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
        var allCommandNodes = context.SyntaxProvider.ForAttributeWithMetadataName(
            SymbolNames.CliFxCommandAttribute,
            predicate: static (node, _) => node is ClassDeclarationSyntax,
            transform: static (ctx, _) =>
                (
                    ClassDeclaration: (ClassDeclarationSyntax)ctx.TargetNode,
                    Symbol: (INamedTypeSymbol)ctx.TargetSymbol
                )
        );

        // Emit diagnostics for [Command] classes that are not partial or don't implement ICommand
        var diagnostics = allCommandNodes.SelectMany(
            static (item, _) =>
            {
                // Abstract classes are intentionally skipped — no diagnostic needed
                if (item.Symbol.IsAbstract)
                    return [];

                if (!item.ClassDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                    return
                    [
                        Diagnostic.Create(
                            DiagnosticDescriptors.CommandMustBePartial,
                            item.ClassDeclaration.Identifier.GetLocation(),
                            item.Symbol.Name
                        ),
                    ];

                if (!item.Symbol.ImplementsInterface(SymbolNames.CliFxCommandInterface))
                    return
                    [
                        Diagnostic.Create(
                            DiagnosticDescriptors.CommandMustImplementICommand,
                            item.ClassDeclaration.Identifier.GetLocation(),
                            item.Symbol.Name
                        ),
                    ];

                return Array.Empty<Diagnostic>();
            }
        );
        context.RegisterSourceOutput(
            diagnostics,
            static (ctx, diagnostic) => ctx.ReportDiagnostic(diagnostic)
        );

        // Only process classes that are partial, implement ICommand, and are not abstract
        var commandDeclarations = allCommandNodes
            .Select(
                static (item, _) =>
                {
                    if (
                        !item.ClassDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
                        || item.Symbol.IsAbstract
                        || !item.Symbol.ImplementsInterface(SymbolNames.CliFxCommandInterface)
                    )
                        return null;

                    return TryBuildCommandDescriptor(item.Symbol);
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

        var allProperties = GetAllProperties(type);

        var parameters = new List<CommandParameterDescriptor>();
        var options = new List<CommandOptionDescriptor>();
        var diagnostics = new List<Diagnostic>();

        // Returns all non-static properties declared on the type and its base classes,
        // with derived class properties shadowing base class properties of the same name.
        static IEnumerable<IPropertySymbol> GetAllProperties(INamedTypeSymbol t)
        {
            var seen = new HashSet<string>();
            var current = (INamedTypeSymbol?)t;
            while (current is not null && current.SpecialType != SpecialType.System_Object)
            {
                foreach (var prop in current.GetMembers().OfType<IPropertySymbol>())
                {
                    if (!prop.IsStatic && seen.Add(prop.Name))
                        yield return prop;
                }

                current = current.BaseType;
            }
        }

        foreach (var property in allProperties)
        {
            var paramAttr = property
                .GetAttributes()
                .FirstOrDefault(a =>
                    a.AttributeClass?.DisplayNameMatches(SymbolNames.CliFxCommandParameterAttribute)
                    == true
                );

            if (paramAttr is not null)
            {
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
                            DiagnosticDescriptors.ParameterMustHaveName,
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

        return new CommandDescriptor(type, name, description, parameters, options, diagnostics);
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
        var containingNs = command.Type.ContainingNamespace;
        var ns =
            containingNs is not null && !containingNs.IsGlobalNamespace
                ? containingNs.ToDisplayString()
                : null;

        // Collect containing types from outermost to innermost (for nested class support)
        var containingTypes = new List<INamedTypeSymbol>();
        var ct = command.Type.ContainingType;
        while (ct is not null)
        {
            containingTypes.Insert(0, ct);
            ct = ct.ContainingType;
        }

        var sb = new StringBuilder();

        sb.Append(
            // lang=csharp
            """
            // <auto-generated/>
            using CliFx.Extensibility;
            using CliFx.Schema;

            """
        );

        if (ns is not null)
        {
            sb.Append(
                // lang=csharp
                $$"""
                namespace {{ns}};

                """
            );
        }

        // Open containing type wrappers (for nested classes)
        foreach (var containingType in containingTypes)
        {
            // Use MinimallyQualifiedFormat to include type parameters (e.g., Foo<T>) without namespace
            sb.AppendLine(
                $"partial class {containingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}"
            );
            sb.AppendLine("{");
        }

        // Build interface list inline
        var interfaces = new List<string>();
        // Every command gets a help option unless the user already defined one
        var needsHelpOption = !command.Options.Any(o =>
            string.Equals(o.Name, "help", StringComparison.OrdinalIgnoreCase) || o.ShortName == 'h'
        );
        // Only default commands get a version option, unless the user already defined one
        var needsVersionOption =
            string.IsNullOrWhiteSpace(command.Name)
            && !command.Options.Any(o =>
                string.Equals(o.Name, "version", StringComparison.OrdinalIgnoreCase)
            );

        if (needsHelpOption)
            interfaces.Add("global::CliFx.ICommandWithHelpOption");
        if (needsVersionOption)
            interfaces.Add("global::CliFx.ICommandWithVersionOption");
        var interfaceList =
            interfaces.Count > 0 ? " : " + string.Join(", ", interfaces) : string.Empty;

        sb.Append(
            // lang=csharp
            $$"""
            partial class {{command.Type.Name}}{{interfaceList}}
            {
            """
        );

        if (needsHelpOption)
        {
            sb.Append(
                // lang=csharp
                """

                    /// <summary>Whether the user requested help (via the -h|--help option).</summary>
                    public bool IsHelpRequested { get; set; }

                """
            );
        }

        if (needsVersionOption)
        {
            sb.Append(
                // lang=csharp
                """

                    /// <summary>Whether the user requested version information (via the --version option).</summary>
                    public bool IsVersionRequested { get; set; }

                """
            );
        }

        sb.Append(
            // lang=csharp
            $$"""

                /// <summary>Generated command schema for <see cref="{{command.Type.Name}}"/>.</summary>
                public static global::CliFx.Schema.CommandSchema Schema { get; } =
                    new global::CliFx.Schema.CommandSchema<{{commandFqn}}>(
                        {{EscapeString(command.Name)}},
                        {{EscapeString(command.Description)}},
                        new global::CliFx.Schema.CommandInputSchema[]
                        {
            """
        );

        foreach (var param in command.Parameters.OrderBy(p => p.Order))
        {
            var propTypeFqn = param.Property.Type.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
            );
            var isSequence = IsSequenceType(param.Property.Type);
            var converterExpr = GetConverterExpression(param.ConverterType, param.Property);

            if (!isSequence)
            {
                sb.Append(
                    // lang=csharp
                    $$"""
                                new global::CliFx.Schema.CommandParameterSchema<{{commandFqn}}, {{propTypeFqn}}>(
                                    new global::CliFx.Schema.PropertyBinding<{{commandFqn}}, {{propTypeFqn}}>(
                                        "{{param.Property.Name}}",
                                        c => c.{{param.Property.Name}},
                                        (c, v) => c.{{param.Property.Name}} = v),
                                    false,
                                    {{param.Order}},
                                    {{EscapeString(param.Name)}},
                                    {{(param.IsRequired ? "true" : "false")}},
                                    {{EscapeString(param.Description)}},
                                    {{converterExpr}},
                                    null,
                                    {{BuildValidatorsExpr(param.ValidatorTypes)}}),

                    """
                );
            }
            else
            {
                var sequenceConverterExpr = BuildSequenceConverterExpr(
                    param.ConverterType,
                    param.Property
                );
                sb.Append(
                    // lang=csharp
                    $$"""
                                new global::CliFx.Schema.CommandParameterSchema<{{commandFqn}}, {{propTypeFqn}}>(
                                    new global::CliFx.Schema.PropertyBinding<{{commandFqn}}, {{propTypeFqn}}>(
                                        "{{param.Property.Name}}",
                                        c => c.{{param.Property.Name}},
                                        (c, v) => c.{{param.Property.Name}} = v),
                                    true,
                                    {{param.Order}},
                                    {{EscapeString(param.Name)}},
                                    {{(param.IsRequired ? "true" : "false")}},
                                    {{EscapeString(param.Description)}},
                                    null,
                                    {{sequenceConverterExpr}},
                                    {{BuildValidatorsExpr(param.ValidatorTypes)}}),

                    """
                );
            }
        }

        foreach (var opt in command.Options)
        {
            var propTypeFqn = opt.Property.Type.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
            );
            var isSequence = IsSequenceType(opt.Property.Type);
            var shortNameStr = opt.ShortName.HasValue ? $"'{opt.ShortName}'" : "null";
            var converterExpr = GetConverterExpression(opt.ConverterType, opt.Property);

            if (!isSequence)
            {
                sb.Append(
                    // lang=csharp
                    $$"""
                                new global::CliFx.Schema.CommandOptionSchema<{{commandFqn}}, {{propTypeFqn}}>(
                                    new global::CliFx.Schema.PropertyBinding<{{commandFqn}}, {{propTypeFqn}}>(
                                        "{{opt.Property.Name}}",
                                        c => c.{{opt.Property.Name}},
                                        (c, v) => c.{{opt.Property.Name}} = v),
                                    false,
                                    {{EscapeString(opt.Name)}},
                                    {{shortNameStr}},
                                    {{EscapeString(opt.EnvironmentVariable)}},
                                    {{(opt.IsRequired ? "true" : "false")}},
                                    {{EscapeString(opt.Description)}},
                                    {{converterExpr}},
                                    null,
                                    {{BuildValidatorsExpr(opt.ValidatorTypes)}}),

                    """
                );
            }
            else
            {
                var sequenceConverterExpr = BuildSequenceConverterExpr(
                    opt.ConverterType,
                    opt.Property
                );
                sb.Append(
                    // lang=csharp
                    $$"""
                                new global::CliFx.Schema.CommandOptionSchema<{{commandFqn}}, {{propTypeFqn}}>(
                                    new global::CliFx.Schema.PropertyBinding<{{commandFqn}}, {{propTypeFqn}}>(
                                        "{{opt.Property.Name}}",
                                        c => c.{{opt.Property.Name}},
                                        (c, v) => c.{{opt.Property.Name}} = v),
                                    true,
                                    {{EscapeString(opt.Name)}},
                                    {{shortNameStr}},
                                    {{EscapeString(opt.EnvironmentVariable)}},
                                    {{(opt.IsRequired ? "true" : "false")}},
                                    {{EscapeString(opt.Description)}},
                                    null,
                                    {{sequenceConverterExpr}},
                                    {{BuildValidatorsExpr(opt.ValidatorTypes)}}),

                    """
                );
            }
        }

        if (needsHelpOption)
        {
            sb.Append(
                // lang=csharp
                $$"""
                            new global::CliFx.Schema.CommandOptionSchema<{{commandFqn}}, bool>(
                                new global::CliFx.Schema.PropertyBinding<{{commandFqn}}, bool>(
                                    "IsHelpRequested",
                                    c => c.IsHelpRequested,
                                    (c, v) => c.IsHelpRequested = v),
                                false,
                                "help",
                                'h',
                                null,
                                false,
                                "Shows help text.",
                                new global::CliFx.Extensibility.BoolBindingConverter(),
                                null,
                                global::System.Array.Empty<global::CliFx.Extensibility.IBindingValidator>()),

                """
            );
        }

        if (needsVersionOption)
        {
            sb.Append(
                // lang=csharp
                $$"""
                            new global::CliFx.Schema.CommandOptionSchema<{{commandFqn}}, bool>(
                                new global::CliFx.Schema.PropertyBinding<{{commandFqn}}, bool>(
                                    "IsVersionRequested",
                                    c => c.IsVersionRequested,
                                    (c, v) => c.IsVersionRequested = v),
                                false,
                                "version",
                                null,
                                null,
                                false,
                                "Shows version information.",
                                new global::CliFx.Extensibility.BoolBindingConverter(),
                                null,
                                global::System.Array.Empty<global::CliFx.Extensibility.IBindingValidator>()),

                """
            );
        }

        sb.Append(
            // lang=csharp
            """
                        }); // end of CommandInputSchema[] array initializer
                } // end of partial class
            """
        );

        // Close containing type wrappers (for nested classes)
        foreach (var _ in containingTypes)
        {
            sb.AppendLine();
            sb.AppendLine("}");
        }

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

    private static string GetConverterExpression(
        TypeDescriptor? converterType,
        IPropertySymbol property
    )
    {
        if (converterType is not null)
            return $"new {converterType.FullyQualifiedName}()";

        var type = property.Type;

        // For sequence types, use the element type's converter
        if (IsSequenceType(type))
        {
            var elementType = type.TryGetEnumerableUnderlyingType();
            if (elementType is not null)
                return BuildDefaultConverterExprForScalar(elementType) ?? "null";
        }

        return BuildDefaultConverterExprForScalar(type) ?? "null";
    }

    private static string BuildValidatorsExpr(IReadOnlyList<TypeDescriptor> validatorTypes)
    {
        if (validatorTypes.Count == 0)
            return "global::System.Array.Empty<global::CliFx.Extensibility.IBindingValidator>()";

        var items = string.Join(", ", validatorTypes.Select(v => $"new {v.FullyQualifiedName}()"));
        return $"new global::CliFx.Extensibility.IBindingValidator[] {{ {items} }}";
    }

    private static string? BuildDefaultConverterExprForScalar(ITypeSymbol type)
    {
        var fqn = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // string — no conversion needed (null converter = pass-through)
        if (type.SpecialType == SpecialType.System_String)
            return null;

        // object — assignable from string; null converter passes raw string through as object
        if (type.SpecialType == SpecialType.System_Object)
            return null;

        // bool
        if (type.SpecialType == SpecialType.System_Boolean)
            return "new global::CliFx.Extensibility.BoolBindingConverter()";

        // DateTimeOffset
        if (type is INamedTypeSymbol { ContainingNamespace.Name: "System", Name: "DateTimeOffset" })
            return "new global::CliFx.Extensibility.DateTimeOffsetBindingConverter()";

        // TimeSpan
        if (type is INamedTypeSymbol { ContainingNamespace.Name: "System", Name: "TimeSpan" })
            return "new global::CliFx.Extensibility.TimeSpanBindingConverter()";

        // Enum
        if (type.TypeKind == TypeKind.Enum)
            return $"new global::CliFx.Extensibility.EnumBindingConverter<{fqn}>()";

        // Nullable<T>
        if (
            type is INamedTypeSymbol { IsValueType: true } named
            && named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T
        )
        {
            var innerType = named.TypeArguments[0];
            var innerFqn = innerType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var innerConverterExpr = BuildDefaultConverterExprForScalar(innerType);
            if (innerConverterExpr is null)
                return null;
            return $"new global::CliFx.Extensibility.NullableBindingConverter<{innerFqn}>({innerConverterExpr})";
        }

        // IConvertible (int, double, char, etc.)
        if (
            type.AllInterfaces.Any(i =>
                i.ContainingNamespace?.Name == "System" && i.Name == "IConvertible"
            )
        )
            return $"new global::CliFx.Extensibility.ConvertibleBindingConverter<{fqn}>()";

        // String-parseable with IFormatProvider: static T Parse(string, IFormatProvider)
        var parseWithFormatProvider = type.GetMembers("Parse")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m =>
                m.IsStatic
                && m.DeclaredAccessibility == Accessibility.Public
                && m.Parameters.Length == 2
                && m.Parameters[0].Type.SpecialType == SpecialType.System_String
                && m.Parameters[1].Type
                    is { ContainingNamespace.Name: "System", Name: "IFormatProvider" }
                && SymbolEqualityComparer.Default.Equals(m.ReturnType, type)
            );
        if (parseWithFormatProvider is not null)
            return $"new global::CliFx.Extensibility.DelegateBindingConverter<{fqn}>(s => {fqn}.Parse(s!, global::System.Globalization.CultureInfo.InvariantCulture))";

        // String-parseable: static T Parse(string)
        var parseMethod = type.GetMembers("Parse")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m =>
                m.IsStatic
                && m.DeclaredAccessibility == Accessibility.Public
                && m.Parameters.Length == 1
                && m.Parameters[0].Type.SpecialType == SpecialType.System_String
                && SymbolEqualityComparer.Default.Equals(m.ReturnType, type)
            );
        if (parseMethod is not null)
            return $"new global::CliFx.Extensibility.DelegateBindingConverter<{fqn}>(s => {fqn}.Parse(s!))";

        // String-constructable: public constructor(string)
        if (
            type is INamedTypeSymbol namedConstructable
            && namedConstructable.Constructors.Any(c =>
                c.DeclaredAccessibility == Accessibility.Public
                && c.Parameters.Length == 1
                && c.Parameters[0].Type.SpecialType == SpecialType.System_String
            )
        )
            return $"new global::CliFx.Extensibility.DelegateBindingConverter<{fqn}>(s => new {fqn}(s!))";

        return null;
    }

    // Builds a collection converter expression for a sequence property.
    // Returns "null" if the collection type is not supported for AOT-compatible generation.
    private static string BuildSequenceConverterExpr(
        TypeDescriptor? userConverterType,
        IPropertySymbol property
    )
    {
        var collectionType = property.Type;
        var elementType = collectionType.TryGetEnumerableUnderlyingType();
        if (elementType is null)
            return "null";

        var collectionTypeFqn = collectionType.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat
        );
        var elementTypeFqn = elementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // Get element-level converter expression (null means string pass-through)
        string? elementConverterExpr;
        if (userConverterType is not null)
            elementConverterExpr = $"new {userConverterType.FullyQualifiedName}()";
        else
            elementConverterExpr = BuildDefaultConverterExprForScalar(elementType);

        var elementConverterArg = elementConverterExpr ?? "null";

        // T[] — use the single-type-parameter convenience form
        if (collectionType is IArrayTypeSymbol)
            return $"new global::CliFx.Extensibility.ArraySequenceBindingConverter<{elementTypeFqn}>({elementConverterArg})";

        // Interface (IReadOnlyList<T>, IEnumerable<T>, etc.) — T[] is assignable to any of these;
        // use the two-type-parameter form so the result type matches TSequence directly.
        if (collectionType.TypeKind == TypeKind.Interface)
            return $"new global::CliFx.Extensibility.ArraySequenceBindingConverter<{elementTypeFqn}, {collectionTypeFqn}>({elementConverterArg})";

        // Concrete type with IEnumerable<T> or T[] constructor (e.g., List<T>)
        if (
            collectionType is INamedTypeSymbol namedCollection
            && namedCollection.Constructors.Any(c =>
                c.DeclaredAccessibility == Accessibility.Public
                && c.Parameters.Length == 1
                && (
                    (
                        c.Parameters[0].Type is IArrayTypeSymbol arrParam
                        && SymbolEqualityComparer.Default.Equals(arrParam.ElementType, elementType)
                    )
                    || (
                        c.Parameters[0].Type is INamedTypeSymbol paramType
                        && paramType.ConstructedFrom.SpecialType
                            == SpecialType.System_Collections_Generic_IEnumerable_T
                        && SymbolEqualityComparer.Default.Equals(
                            paramType.TypeArguments[0],
                            elementType
                        )
                    )
                )
            )
        )
        {
            return $"new global::CliFx.Extensibility.ArrayInitializableSequenceBindingConverter<{elementTypeFqn}, {collectionTypeFqn}>({elementConverterArg}, arr => new {collectionTypeFqn}(arr))";
        }

        // Unknown collection type — user must provide a custom ISequenceBindingConverter
        return "null";
    }
}
