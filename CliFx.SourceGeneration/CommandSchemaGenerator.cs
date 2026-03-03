using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CliFx.SourceGeneration.SemanticModel;
using CliFx.SourceGeneration.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CliFx.SourceGeneration;

/// <summary>
/// Source generator that generates strongly-typed command schema registration code for CliFx commands.
/// </summary>
[Generator]
public class CommandSchemaGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var commandDeclarations = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) =>
                    node is ClassDeclarationSyntax cls
                    && cls.Modifiers.IndexOf(SyntaxKind.PartialKeyword) >= 0
                    && cls.AttributeLists.Count > 0,
                transform: static (ctx, cancellationToken) =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)ctx.Node;
                    var symbol = ctx.SemanticModel.GetDeclaredSymbol(
                        classDeclaration,
                        cancellationToken
                    );
                    if (symbol is not INamedTypeSymbol typeSymbol)
                        return null;

                    return TryBuildCommandDescriptor(typeSymbol);
                }
            )
            .WhereNotNull();

        context.RegisterSourceOutput(
            commandDeclarations.Collect(),
            static (ctx, commands) => Execute(ctx, commands)
        );
    }

    internal static CommandDescriptor? TryBuildCommandDescriptor(INamedTypeSymbol type)
    {
        // Must implement ICommand
        if (!type.ImplementsInterface(SymbolNames.CliFxCommandInterface))
            return null;

        // Must have [Command] attribute
        var commandAttr = type.GetAttributes()
            .FirstOrDefault(a =>
                a.AttributeClass?.DisplayNameMatches(SymbolNames.CliFxCommandAttribute) == true
            );
        if (commandAttr is null)
            return null;

        // Must be a concrete class
        if (type.IsAbstract)
            return null;

        var name =
            commandAttr
                .NamedArguments.Where(a => a.Key == "Name")
                .Select(a => a.Value.Value as string)
                .FirstOrDefault()
            ?? commandAttr
                .ConstructorArguments.Where(a => a.Type?.SpecialType == SpecialType.System_String)
                .Select(a => a.Value as string)
                .FirstOrDefault();

        var description = commandAttr
            .NamedArguments.Where(a => a.Key == "Description")
            .Select(a => a.Value.Value as string)
            .FirstOrDefault();

        var properties = type.GetMembers().OfType<IPropertySymbol>().ToArray();

        var parameters = new List<CommandParameterDescriptor>();
        var options = new List<CommandOptionDescriptor>();
        var skippedInitOnly = new List<IPropertySymbol>();
        var diagnostics = new List<Diagnostic>();

        foreach (var property in properties)
        {
            var paramAttr = property
                .GetAttributes()
                .FirstOrDefault(a =>
                    a.AttributeClass?.DisplayNameMatches(SymbolNames.CliFxCommandParameterAttribute)
                    == true
                );

            if (paramAttr is not null)
            {
                // Skip init-only properties — generated setter lambda (c, v) => c.Prop = v
                // does not compile for init accessors.
                if (property.SetMethod?.IsInitOnly == true)
                {
                    skippedInitOnly.Add(property);
                    continue;
                }

                var order = paramAttr
                    .ConstructorArguments.Where(a =>
                        a.Type?.SpecialType == SpecialType.System_Int32
                    )
                    .Select(a => (int)(a.Value ?? 0))
                    .FirstOrDefault();

                var paramName =
                    paramAttr
                        .NamedArguments.Where(a => a.Key == "Name")
                        .Select(a => a.Value.Value as string)
                        .FirstOrDefault()
                    ?? property.Name.ToLowerInvariant();

                // CLIFX005: parameter name must not be empty
                var explicitName = paramAttr
                    .NamedArguments.Where(a => a.Key == "Name")
                    .Select(a => a.Value.Value as string)
                    .FirstOrDefault();
                if (explicitName is not null && string.IsNullOrWhiteSpace(explicitName))
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.ParameterNameEmpty,
                            property.Locations.FirstOrDefault() ?? Location.None,
                            property.Name
                        )
                    );
                }

                var paramIsRequired =
                    paramAttr
                        .NamedArguments.Where(a => a.Key == "IsRequired")
                        .Select(a => a.Value.Value as bool?)
                        .FirstOrDefault()
                    ?? property.IsRequired();

                var paramDescription = paramAttr
                    .NamedArguments.Where(a => a.Key == "Description")
                    .Select(a => a.Value.Value as string)
                    .FirstOrDefault();

                var paramConverterType = paramAttr
                    .NamedArguments.Where(a => a.Key == "Converter")
                    .Select(a =>
                        a.Value.Value is ITypeSymbol sym ? TypeDescriptor.FromSymbol(sym) : null
                    )
                    .FirstOrDefault();

                var paramValidatorTypes = paramAttr
                    .NamedArguments.Where(a => a.Key == "Validators")
                    .SelectMany(a => a.Value.Values)
                    .Select(v => v.Value is ITypeSymbol sym ? TypeDescriptor.FromSymbol(sym) : null)
                    .Where(t => t is not null)
                    .Select(t => t!)
                    .ToArray();

                parameters.Add(
                    new CommandParameterDescriptor(
                        property,
                        order,
                        paramName,
                        paramIsRequired,
                        paramDescription,
                        paramConverterType,
                        paramValidatorTypes
                    )
                );

                continue;
            }

            var optAttr = property
                .GetAttributes()
                .FirstOrDefault(a =>
                    a.AttributeClass?.DisplayNameMatches(SymbolNames.CliFxCommandOptionAttribute)
                    == true
                );

            if (optAttr is not null)
            {
                // Skip init-only properties — generated setter lambda (c, v) => c.Prop = v
                // does not compile for init accessors.
                if (property.SetMethod?.IsInitOnly == true)
                {
                    skippedInitOnly.Add(property);
                    continue;
                }

                var optName =
                    optAttr
                        .ConstructorArguments.Where(a =>
                            a.Type?.SpecialType == SpecialType.System_String
                        )
                        .Select(a => a.Value as string)
                        .FirstOrDefault()
                    ?? optAttr
                        .NamedArguments.Where(a => a.Key == "Name")
                        .Select(a => a.Value.Value as string)
                        .FirstOrDefault();

                var shortName =
                    optAttr
                        .ConstructorArguments.Where(a =>
                            a.Type?.SpecialType == SpecialType.System_Char
                        )
                        .Select(a => a.Value as char?)
                        .FirstOrDefault()
                    ?? optAttr
                        .NamedArguments.Where(a => a.Key == "ShortName")
                        .Select(a => a.Value.Value as char?)
                        .FirstOrDefault();

                // CLIFX003: option must have a name or short name
                if (string.IsNullOrWhiteSpace(optName) && shortName is null)
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.OptionMustHaveNameOrShortName,
                            property.Locations.FirstOrDefault() ?? Location.None,
                            property.Name
                        )
                    );
                }

                // CLIFX004: option name must be valid
                if (!string.IsNullOrWhiteSpace(optName))
                {
                    if (
                        optName.Length < 2
                        || optName.StartsWith("-", StringComparison.Ordinal)
                        || optName.Any(char.IsWhiteSpace)
                    )
                    {
                        diagnostics.Add(
                            Diagnostic.Create(
                                DiagnosticDescriptors.OptionNameInvalid,
                                property.Locations.FirstOrDefault() ?? Location.None,
                                optName,
                                property.Name
                            )
                        );
                    }
                }

                var optIsRequired =
                    optAttr
                        .NamedArguments.Where(a => a.Key == "IsRequired")
                        .Select(a => a.Value.Value as bool?)
                        .FirstOrDefault()
                    ?? property.IsRequired();

                var optDescription = optAttr
                    .NamedArguments.Where(a => a.Key == "Description")
                    .Select(a => a.Value.Value as string)
                    .FirstOrDefault();

                var envVar = optAttr
                    .NamedArguments.Where(a => a.Key == "EnvironmentVariable")
                    .Select(a => a.Value.Value as string)
                    .FirstOrDefault();

                var optConverterType = optAttr
                    .NamedArguments.Where(a => a.Key == "Converter")
                    .Select(a =>
                        a.Value.Value is ITypeSymbol sym ? TypeDescriptor.FromSymbol(sym) : null
                    )
                    .FirstOrDefault();

                var optValidatorTypes = optAttr
                    .NamedArguments.Where(a => a.Key == "Validators")
                    .SelectMany(a => a.Value.Values)
                    .Select(v => v.Value is ITypeSymbol sym ? TypeDescriptor.FromSymbol(sym) : null)
                    .Where(t => t is not null)
                    .Select(t => t!)
                    .ToArray();

                options.Add(
                    new CommandOptionDescriptor(
                        property,
                        optName,
                        shortName,
                        envVar,
                        optIsRequired,
                        optDescription,
                        optConverterType,
                        optValidatorTypes
                    )
                );
            }
        }

        // CLIFX006: options must have unique names and short names
        for (var i = 0; i < options.Count; i++)
        {
            for (var j = i + 1; j < options.Count; j++)
            {
                var a = options[i];
                var b = options[j];

                if (
                    !string.IsNullOrWhiteSpace(a.Name)
                    && string.Equals(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)
                )
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.OptionsMustHaveUniqueNames,
                            b.Property.Locations.FirstOrDefault() ?? Location.None,
                            b.Property.Name,
                            type.Name,
                            "name",
                            b.Name,
                            a.Property.Name
                        )
                    );
                }

                if (a.ShortName is not null && a.ShortName == b.ShortName)
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.OptionsMustHaveUniqueNames,
                            b.Property.Locations.FirstOrDefault() ?? Location.None,
                            b.Property.Name,
                            type.Name,
                            "short name",
                            b.ShortName.ToString(),
                            a.Property.Name
                        )
                    );
                }
            }
        }

        // CLIFX007: parameters must have unique order values
        for (var i = 0; i < parameters.Count; i++)
        {
            for (var j = i + 1; j < parameters.Count; j++)
            {
                var a = parameters[i];
                var b = parameters[j];

                if (a.Order == b.Order)
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.ParametersMustHaveUniqueOrder,
                            b.Property.Locations.FirstOrDefault() ?? Location.None,
                            b.Property.Name,
                            type.Name,
                            b.Order,
                            a.Property.Name
                        )
                    );
                }
            }
        }

        // CLIFX008: parameters must have unique names
        for (var i = 0; i < parameters.Count; i++)
        {
            for (var j = i + 1; j < parameters.Count; j++)
            {
                var a = parameters[i];
                var b = parameters[j];

                if (string.Equals(a.Name, b.Name, StringComparison.OrdinalIgnoreCase))
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.ParametersMustHaveUniqueNames,
                            b.Property.Locations.FirstOrDefault() ?? Location.None,
                            b.Property.Name,
                            type.Name,
                            b.Name,
                            a.Property.Name
                        )
                    );
                }
            }
        }

        return new CommandDescriptor(
            type,
            name,
            description,
            parameters,
            options,
            properties,
            skippedInitOnly,
            diagnostics
        );
    }

    private static void Execute(
        SourceProductionContext ctx,
        IReadOnlyList<CommandDescriptor> commands
    )
    {
        if (commands.Count == 0)
            return;

        foreach (var command in commands)
        {
            if (command.UserDefinedProperties.Any(p => p.Name == "Schema"))
            {
                var schemaLocation =
                    command
                        .Type.GetMembers("Schema")
                        .FirstOrDefault(m =>
                            m.Kind == SymbolKind.Field
                            || m.Kind == SymbolKind.Property
                            || m.Kind == SymbolKind.Method
                        )
                        ?.Locations.FirstOrDefault()
                    ?? Location.None;

                ctx.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.SchemaPropertyAlreadyDefined,
                        schemaLocation,
                        command.Type.Name
                    )
                );
            }

            foreach (var skippedProp in command.SkippedInitOnlyProperties)
            {
                ctx.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.InitOnlyProperty,
                        skippedProp.Locations.FirstOrDefault() ?? Location.None,
                        skippedProp.Name,
                        command.Type.Name
                    )
                );
            }

            foreach (var diagnostic in command.Diagnostics)
            {
                ctx.ReportDiagnostic(diagnostic);
            }

            var source = GenerateCommandSchemaSource(command);
            var hintName = $"{command.Type.ToDisplayString().Replace('.', '_')}_Schema.g.cs";
            ctx.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string GenerateCommandSchemaSource(CommandDescriptor command)
    {
        var commandFqn = command.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var ns = command.Type.ContainingNamespace?.ToDisplayString();

        var sb = new StringBuilder();

        sb.Append(
            // lang=csharp
            """
            // <auto-generated/>
            using CliFx.Extensibility;
            using CliFx.Schema;

            """
        );

        if (!string.IsNullOrEmpty(ns))
        {
            sb.Append(
                // lang=csharp
                $$"""
                namespace {{ns}};

                """
            );
        }

        sb.Append(
            // lang=csharp
            $$"""
            partial class {{command.Type.Name}}
            {
            """
        );

        if (!command.UserDefinedProperties.Any(p => p.Name == "Schema"))
        {
            sb.Append(
                // lang=csharp
                $$"""

                    /// <summary>Generated command schema for <see cref="{{command.Type.Name}}"/>.</summary>
                    public static global::CliFx.Schema.CommandSchema Schema { get; } = BuildSchema();

                """
            );
        }

        sb.Append(
            // lang=csharp
            """
                private static global::CliFx.Schema.CommandSchema BuildSchema()
                {
                    var inputs = new global::System.Collections.Generic.List<global::CliFx.Schema.CommandInputSchema>();

            """
        );

        foreach (var param in command.Parameters.OrderBy(p => p.Order))
        {
            var propTypeFqn = param.Property.Type.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
            );
            var isSequence = IsSequenceType(param.Property.Type);

            sb.Append(
                // lang=csharp
                $$"""
                        inputs.Add(new global::CliFx.Schema.CommandParameterSchema<{{commandFqn}}, {{propTypeFqn}}>(
                            new global::CliFx.Schema.PropertyBinding<{{commandFqn}}, {{propTypeFqn}}>(
                                c => c.{{param.Property.Name}},
                                (c, v) => c.{{param.Property.Name}} = v),
                            {{(isSequence ? "true" : "false")}},
                            {{param.Order}},
                            {{EscapeString(param.Name)}},
                            {{(param.IsRequired ? "true" : "false")}},
                            {{EscapeString(param.Description)}},
                            {{BuildConverterExpr(param.ConverterType)}},
                            {{BuildValidatorsExpr(param.ValidatorTypes)}}));

                """
            );
        }

        foreach (var opt in command.Options)
        {
            var propTypeFqn = opt.Property.Type.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
            );
            var isSequence = IsSequenceType(opt.Property.Type);
            var shortNameStr = opt.ShortName.HasValue ? $"'{opt.ShortName}'" : "null";

            sb.Append(
                // lang=csharp
                $$"""
                        inputs.Add(new global::CliFx.Schema.CommandOptionSchema<{{commandFqn}}, {{propTypeFqn}}>(
                            new global::CliFx.Schema.PropertyBinding<{{commandFqn}}, {{propTypeFqn}}>(
                                c => c.{{opt.Property.Name}},
                                (c, v) => c.{{opt.Property.Name}} = v),
                            {{(isSequence ? "true" : "false")}},
                            {{EscapeString(opt.Name)}},
                            {{shortNameStr}},
                            {{EscapeString(opt.EnvironmentVariable)}},
                            {{(opt.IsRequired ? "true" : "false")}},
                            {{EscapeString(opt.Description)}},
                            {{BuildConverterExpr(opt.ConverterType)}},
                            {{BuildValidatorsExpr(opt.ValidatorTypes)}}));

                """
            );
        }

        sb.Append(
            // lang=csharp
            $$"""
                    return new global::CliFx.Schema.CommandSchema<{{commandFqn}}>(
                        {{EscapeString(command.Name)}},
                        {{EscapeString(command.Description)}},
                        inputs);
                }
            }
            """
        );

        return sb.ToString();
    }

    private static bool IsSequenceType(ITypeSymbol type)
    {
        if (type.SpecialType == SpecialType.System_String)
            return false;

        return type.AllInterfaces.Any(i =>
            i.ConstructedFrom.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T
        );
    }

    private static string EscapeString(string? value) =>
        value is null ? "null" : $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";

    private static string BuildConverterExpr(TypeDescriptor? converterType)
    {
        if (converterType is null)
            return "null";
        return $"new {converterType.FullyQualifiedName}()";
    }

    private static string BuildValidatorsExpr(IReadOnlyList<TypeDescriptor> validatorTypes)
    {
        if (validatorTypes.Count == 0)
            return "global::System.Array.Empty<global::CliFx.Extensibility.IBindingValidator>()";

        var items = string.Join(", ", validatorTypes.Select(v => $"new {v.FullyQualifiedName}()"));
        return $"new global::CliFx.Extensibility.IBindingValidator[] {{ {items} }}";
    }
}
