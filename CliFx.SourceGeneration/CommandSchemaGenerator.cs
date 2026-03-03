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

        var directProperties = type.GetMembers().OfType<IPropertySymbol>().ToArray();
        var allProperties = GetAllProperties(type);

        var parameters = new List<CommandParameterDescriptor>();
        var options = new List<CommandOptionDescriptor>();
        var skippedInitOnly = new List<IPropertySymbol>();
        var diagnostics = new List<Diagnostic>();

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

        var needsHelpOption = !options.Any(o =>
            string.Equals(o.Name, "help", StringComparison.OrdinalIgnoreCase) || o.ShortName == 'h'
        );
        var needsVersionOption =
            string.IsNullOrWhiteSpace(name)
            && !options.Any(o =>
                string.Equals(o.Name, "version", StringComparison.OrdinalIgnoreCase)
            );

        return new CommandDescriptor(
            type,
            name,
            description,
            parameters,
            options,
            directProperties,
            skippedInitOnly,
            diagnostics,
            needsHelpOption,
            needsVersionOption
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
        var containingNs = command.Type.ContainingNamespace;
        var ns =
            containingNs is not null && !containingNs.IsGlobalNamespace
                ? containingNs.ToDisplayString()
                : null;

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

        sb.Append(
            // lang=csharp
            $$"""
            partial class {{command.Type.Name}}{{BuildInterfaceList(command)}}
            {
            """
        );

        if (command.NeedsHelpOption)
        {
            sb.Append(
                // lang=csharp
                """

                    /// <summary>Whether the user requested help (via the -h|--help option).</summary>
                    public bool IsHelpRequested { get; set; }

                """
            );
        }

        if (command.NeedsVersionOption)
        {
            sb.Append(
                // lang=csharp
                """

                    /// <summary>Whether the user requested version information (via the --version option).</summary>
                    public bool IsVersionRequested { get; set; }

                """
            );
        }

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
            var converterExpr =
                param.ConverterType != null
                    ? BuildConverterExpr(param.ConverterType)
                    : BuildDefaultConverterExpr(param.Property);

            if (!isSequence)
            {
                sb.Append(
                    // lang=csharp
                    $$"""
                            inputs.Add(new global::CliFx.Schema.CommandParameterSchema<{{commandFqn}}, {{propTypeFqn}}>(
                                new global::CliFx.Schema.PropertyBinding<{{commandFqn}}, {{propTypeFqn}}>(
                                    c => c.{{param.Property.Name}},
                                    (c, v) => c.{{param.Property.Name}} = v),
                                false,
                                {{param.Order}},
                                {{EscapeString(param.Name)}},
                                {{(param.IsRequired ? "true" : "false")}},
                                {{EscapeString(param.Description)}},
                                {{converterExpr}},
                                {{BuildValidatorsExpr(param.ValidatorTypes)}}));

                    """
                );
            }
            else
            {
                sb.Append(
                    // lang=csharp
                    $$"""
                            inputs.Add(new global::CliFx.Schema.CommandParameterSchema(
                                new global::CliFx.Schema.PropertyBinding<{{commandFqn}}, {{propTypeFqn}}>(
                                    c => c.{{param.Property.Name}},
                                    (c, v) => c.{{param.Property.Name}} = v),
                                true,
                                {{param.Order}},
                                {{EscapeString(param.Name)}},
                                {{(param.IsRequired ? "true" : "false")}},
                                {{EscapeString(param.Description)}},
                                {{converterExpr}},
                                {{BuildValidatorsExpr(param.ValidatorTypes)}}));

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
            var converterExpr =
                opt.ConverterType != null
                    ? BuildConverterExpr(opt.ConverterType)
                    : BuildDefaultConverterExpr(opt.Property);

            if (!isSequence)
            {
                sb.Append(
                    // lang=csharp
                    $$"""
                            inputs.Add(new global::CliFx.Schema.CommandOptionSchema<{{commandFqn}}, {{propTypeFqn}}>(
                                new global::CliFx.Schema.PropertyBinding<{{commandFqn}}, {{propTypeFqn}}>(
                                    c => c.{{opt.Property.Name}},
                                    (c, v) => c.{{opt.Property.Name}} = v),
                                false,
                                {{EscapeString(opt.Name)}},
                                {{shortNameStr}},
                                {{EscapeString(opt.EnvironmentVariable)}},
                                {{(opt.IsRequired ? "true" : "false")}},
                                {{EscapeString(opt.Description)}},
                                {{converterExpr}},
                                {{BuildValidatorsExpr(opt.ValidatorTypes)}}));

                    """
                );
            }
            else
            {
                sb.Append(
                    // lang=csharp
                    $$"""
                            inputs.Add(new global::CliFx.Schema.CommandOptionSchema(
                                new global::CliFx.Schema.PropertyBinding<{{commandFqn}}, {{propTypeFqn}}>(
                                    c => c.{{opt.Property.Name}},
                                    (c, v) => c.{{opt.Property.Name}} = v),
                                true,
                                {{EscapeString(opt.Name)}},
                                {{shortNameStr}},
                                {{EscapeString(opt.EnvironmentVariable)}},
                                {{(opt.IsRequired ? "true" : "false")}},
                                {{EscapeString(opt.Description)}},
                                {{converterExpr}},
                                {{BuildValidatorsExpr(opt.ValidatorTypes)}}));

                    """
                );
            }
        }

        if (command.NeedsHelpOption)
        {
            sb.Append(
                // lang=csharp
                $$"""
                        inputs.Add(new global::CliFx.Schema.CommandOptionSchema<{{commandFqn}}, bool>(
                            new global::CliFx.Schema.PropertyBinding<{{commandFqn}}, bool>(
                                c => c.IsHelpRequested,
                                (c, v) => c.IsHelpRequested = v),
                            false,
                            "help",
                            'h',
                            null,
                            false,
                            "Shows help text.",
                            new global::CliFx.Extensibility.BoolBindingConverter(),
                            global::System.Array.Empty<global::CliFx.Extensibility.IBindingValidator>()));

                """
            );
        }

        if (command.NeedsVersionOption)
        {
            sb.Append(
                // lang=csharp
                $$"""
                        inputs.Add(new global::CliFx.Schema.CommandOptionSchema<{{commandFqn}}, bool>(
                            new global::CliFx.Schema.PropertyBinding<{{commandFqn}}, bool>(
                                c => c.IsVersionRequested,
                                (c, v) => c.IsVersionRequested = v),
                            false,
                            "version",
                            null,
                            null,
                            false,
                            "Shows version information.",
                            new global::CliFx.Extensibility.BoolBindingConverter(),
                            global::System.Array.Empty<global::CliFx.Extensibility.IBindingValidator>()));

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

    // Returns all non-static properties declared on the type and its base classes,
    // with derived class properties shadowing base class properties of the same name.
    private static IEnumerable<IPropertySymbol> GetAllProperties(INamedTypeSymbol type)
    {
        var seen = new HashSet<string>();
        var current = (INamedTypeSymbol?)type;
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

    private static string BuildDefaultConverterExpr(IPropertySymbol property)
    {
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

    private static string? BuildDefaultConverterExprForScalar(ITypeSymbol type)
    {
        var fqn = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // string
        if (type.SpecialType == SpecialType.System_String)
            return "new global::CliFx.Extensibility.NoopBindingConverter()";

        // object — assignable from string, so pass the raw string through as object
        if (type.SpecialType == SpecialType.System_Object)
            return "new global::CliFx.Extensibility.DelegateBindingConverter<global::System.Object>(s => s)";

        // bool
        if (type.SpecialType == SpecialType.System_Boolean)
            return "new global::CliFx.Extensibility.BoolBindingConverter()";

        // DateTimeOffset
        if (type.ToDisplayString() == "System.DateTimeOffset")
            return "new global::CliFx.Extensibility.DateTimeOffsetBindingConverter()";

        // TimeSpan
        if (type.ToDisplayString() == "System.TimeSpan")
            return "new global::CliFx.Extensibility.TimeSpanBindingConverter()";

        // Guid
        if (type.ToDisplayString() == "System.Guid")
            return "new global::CliFx.Extensibility.GuidBindingConverter()";

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
        if (type.AllInterfaces.Any(i => i.ToDisplayString() == "System.IConvertible"))
            return $"new global::CliFx.Extensibility.ConvertibleBindingConverter<{fqn}>()";

        // String-parseable with IFormatProvider: static T Parse(string, IFormatProvider)
        var parseWithFormatProvider = type.GetMembers("Parse")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m =>
                m.IsStatic
                && m.DeclaredAccessibility == Accessibility.Public
                && m.Parameters.Length == 2
                && m.Parameters[0].Type.SpecialType == SpecialType.System_String
                && m.Parameters[1].Type.ToDisplayString() == "System.IFormatProvider"
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

    private static string BuildInterfaceList(CommandDescriptor command)
    {
        var interfaces = new List<string>();

        if (command.NeedsHelpOption)
            interfaces.Add("global::CliFx.ICommandWithHelpOption");

        if (command.NeedsVersionOption)
            interfaces.Add("global::CliFx.ICommandWithVersionOption");

        if (interfaces.Count == 0)
            return string.Empty;

        return " : " + string.Join(", ", interfaces);
    }
}
