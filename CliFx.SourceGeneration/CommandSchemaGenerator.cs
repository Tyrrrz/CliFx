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
    private static readonly DiagnosticDescriptor SchemaPropertyAlreadyDefinedDiagnostic = new(
        id: "CLIFX001",
        title: "Schema property already defined",
        messageFormat: "Type '{0}' already defines a 'Schema' member. The source-generated 'Schema' property will be skipped. Rename or remove the existing member to allow the generator to produce the schema.",
        category: "CliFx",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

    private static readonly DiagnosticDescriptor InitOnlyPropertyDiagnostic = new(
        id: "CLIFX002",
        title: "Init-only property cannot be source-generated",
        messageFormat: "Property '{0}' on type '{1}' is init-only and will be skipped by the source generator. Change it to a regular settable property to enable source-generated binding.",
        category: "CliFx",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );

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

    private static CommandDescriptor? TryBuildCommandDescriptor(INamedTypeSymbol type)
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
                    .Select(a => TypeDescriptor.TryFromSymbol(a.Value.Value as ITypeSymbol))
                    .FirstOrDefault();

                var paramValidatorTypes = paramAttr
                    .NamedArguments.Where(a => a.Key == "Validators")
                    .SelectMany(a => a.Value.Values)
                    .Select(v => TypeDescriptor.TryFromSymbol(v.Value as ITypeSymbol))
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
                    .Select(a => TypeDescriptor.TryFromSymbol(a.Value.Value as ITypeSymbol))
                    .FirstOrDefault();

                var optValidatorTypes = optAttr
                    .NamedArguments.Where(a => a.Key == "Validators")
                    .SelectMany(a => a.Value.Values)
                    .Select(v => TypeDescriptor.TryFromSymbol(v.Value as ITypeSymbol))
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

        return new CommandDescriptor(
            type,
            name,
            description,
            parameters,
            options,
            type.GetMembers("Schema")
                .Any(m =>
                    m.Kind == SymbolKind.Field
                    || m.Kind == SymbolKind.Property
                    || m.Kind == SymbolKind.Method
                ),
            skippedInitOnly
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
            if (command.HasExistingSchemaProperty)
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
                        SchemaPropertyAlreadyDefinedDiagnostic,
                        schemaLocation,
                        command.Type.Name
                    )
                );
            }

            foreach (var skippedProp in command.SkippedInitOnlyProperties)
            {
                ctx.ReportDiagnostic(
                    Diagnostic.Create(
                        InitOnlyPropertyDiagnostic,
                        skippedProp.Locations.FirstOrDefault() ?? Location.None,
                        skippedProp.Name,
                        command.Type.Name
                    )
                );
            }

            var source = GenerateCommandSchemaSource(command);
            var hintName = $"{command.Type.ToDisplayString().Replace('.', '_')}_Schema.g.cs";
            ctx.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string GenerateCommandSchemaSource(CommandDescriptor command)
    {
        var commandFqn = command.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using CliFx.Extensibility;");
        sb.AppendLine("using CliFx.Schema;");
        sb.AppendLine();

        var ns = command.Type.ContainingNamespace?.ToDisplayString();
        if (!string.IsNullOrEmpty(ns))
        {
            sb.AppendLine($"namespace {ns};");
            sb.AppendLine();
        }

        sb.AppendLine($"partial class {command.Type.Name}");
        sb.AppendLine("{");
        if (!command.HasExistingSchemaProperty)
        {
            sb.AppendLine(
                $"    /// <summary>Generated command schema for <see cref=\"{command.Type.Name}\"/>.</summary>"
            );
            sb.AppendLine(
                $"    public static global::CliFx.Schema.CommandSchema Schema {{ get; }} = BuildSchema();"
            );
            sb.AppendLine();
        }
        sb.AppendLine($"    private static global::CliFx.Schema.CommandSchema BuildSchema()");
        sb.AppendLine("    {");

        // Build inputs list
        sb.AppendLine(
            "        var inputs = new global::System.Collections.Generic.List<global::CliFx.Schema.CommandInputSchema>();"
        );
        sb.AppendLine();

        // Parameters
        foreach (var param in command.Parameters.OrderBy(p => p.Order))
        {
            var propTypeFqn = param.Property.Type.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
            );
            var isSequence = IsSequenceType(param.Property.Type);
            var nameStr = EscapeString(param.Name);
            var descStr = EscapeStringOrNull(param.Description);
            var converterExpr = BuildConverterExpr(param.ConverterType);
            var validatorsExpr = BuildValidatorsExpr(param.ValidatorTypes);
            var isRequired = param.IsRequired ? "true" : "false";

            sb.AppendLine(
                $"        inputs.Add(new global::CliFx.Schema.CommandParameterSchema<{commandFqn}, {propTypeFqn}>("
            );
            sb.AppendLine(
                $"            new global::CliFx.Schema.PropertyBinding<{commandFqn}, {propTypeFqn}>("
            );
            sb.AppendLine($"                c => c.{param.Property.Name},");
            sb.AppendLine($"                (c, v) => c.{param.Property.Name} = v),");
            sb.AppendLine($"            {(isSequence ? "true" : "false")},");
            sb.AppendLine($"            {param.Order},");
            sb.AppendLine($"            {nameStr},");
            sb.AppendLine($"            {isRequired},");
            sb.AppendLine($"            {descStr},");
            sb.AppendLine($"            {converterExpr},");
            sb.AppendLine($"            {validatorsExpr}));");
            sb.AppendLine();
        }

        // Options
        foreach (var opt in command.Options)
        {
            var propTypeFqn = opt.Property.Type.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
            );
            var isSequence = IsSequenceType(opt.Property.Type);
            var nameStr = EscapeStringOrNull(opt.Name);
            var shortNameStr = opt.ShortName.HasValue ? $"'{opt.ShortName}'" : "null";
            var envVarStr = EscapeStringOrNull(opt.EnvironmentVariable);
            var descStr = EscapeStringOrNull(opt.Description);
            var converterExpr = BuildConverterExpr(opt.ConverterType);
            var validatorsExpr = BuildValidatorsExpr(opt.ValidatorTypes);
            var isRequired = opt.IsRequired ? "true" : "false";

            sb.AppendLine(
                $"        inputs.Add(new global::CliFx.Schema.CommandOptionSchema<{commandFqn}, {propTypeFqn}>("
            );
            sb.AppendLine(
                $"            new global::CliFx.Schema.PropertyBinding<{commandFqn}, {propTypeFqn}>("
            );
            sb.AppendLine($"                c => c.{opt.Property.Name},");
            sb.AppendLine($"                (c, v) => c.{opt.Property.Name} = v),");
            sb.AppendLine($"            {(isSequence ? "true" : "false")},");
            sb.AppendLine($"            {nameStr},");
            sb.AppendLine($"            {shortNameStr},");
            sb.AppendLine($"            {envVarStr},");
            sb.AppendLine($"            {isRequired},");
            sb.AppendLine($"            {descStr},");
            sb.AppendLine($"            {converterExpr},");
            sb.AppendLine($"            {validatorsExpr}));");
            sb.AppendLine();
        }

        var cmdNameStr = EscapeStringOrNull(command.Name);
        var cmdDescStr = EscapeStringOrNull(command.Description);

        sb.AppendLine($"        return new global::CliFx.Schema.CommandSchema<{commandFqn}>(");
        sb.AppendLine($"            {cmdNameStr},");
        sb.AppendLine($"            {cmdDescStr},");
        sb.AppendLine("            inputs);");
        sb.AppendLine("    }");
        sb.AppendLine("}");

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

    private static string EscapeStringOrNull(string? value) => EscapeString(value);

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
