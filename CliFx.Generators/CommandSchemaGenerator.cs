using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CliFx.Generators.SemanticModel;
using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CliFx.Generators;

/// <summary>
/// Source generator that generates strongly-typed command schema registration code for CliFx commands.
/// </summary>
[Generator]
public class CommandSchemaGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var cliFxRefs = context.CompilationProvider.Select(
            static (compilation, _) => CliFxReferences.From(compilation)
        );

        var commandNodes = context.SyntaxProvider.ForAttributeWithMetadataName(
            CliFxReferences.CommandAttributeMetadataName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (ctx, _) =>
                (
                    ClassDeclaration: (ClassDeclarationSyntax)ctx.TargetNode,
                    Symbol: (INamedTypeSymbol)ctx.TargetSymbol
                )
        );

        var commandNodesWithRefs = commandNodes.Combine(cliFxRefs);

        // Emit diagnostics for [Command] classes that are not partial or don't implement ICommand
        var diagnostics = commandNodesWithRefs.SelectMany(
            static (pair, _) =>
            {
                var (item, refs) = pair;

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

                if (
                    !item.Symbol.AllInterfaces.Any(i =>
                        SymbolEqualityComparer.Default.Equals(i, refs.ICommand.Symbol)
                    )
                )
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
        var commandDeclarations = commandNodesWithRefs
            .Select(
                static (pair, _) =>
                {
                    var (item, refs) = pair;
                    if (
                        !item.ClassDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
                        || item.Symbol.IsAbstract
                        || !item.Symbol.AllInterfaces.Any(i =>
                            SymbolEqualityComparer.Default.Equals(i, refs.ICommand.Symbol)
                        )
                    )
                        return null;

                    return TryBuildCommandDescriptor(item.Symbol, refs);
                }
            )
            .WhereNotNull();

        context.RegisterSourceOutput(
            commandDeclarations.Collect().Combine(cliFxRefs),
            static (ctx, pair) => Execute(ctx, pair.Left, pair.Right)
        );
    }

    internal static CommandDescriptor? TryBuildCommandDescriptor(
        INamedTypeSymbol type,
        CliFxReferences refs
    )
    {
        // Must implement ICommand
        if (
            !type.AllInterfaces.Any(i =>
                SymbolEqualityComparer.Default.Equals(i, refs.ICommand.Symbol)
            )
        )
            return null;

        // Must have [Command] attribute
        var commandAttribute = type.GetAttributes()
            .FirstOrDefault(a =>
                SymbolEqualityComparer.Default.Equals(
                    a.AttributeClass,
                    refs.CommandAttribute.Symbol
                )
            );

        if (commandAttribute is null)
            return null;

        // Must be a concrete class
        if (type.IsAbstract)
            return null;

        var commandName =
            commandAttribute.NamedArguments.FirstOrDefault(a => a.Key == "Name").Value.Value
                as string
            ?? commandAttribute
                .ConstructorArguments.Where(a => a.Type?.SpecialType == SpecialType.System_String)
                .Select(a => a.Value as string)
                .FirstOrDefault();

        var commandDescription =
            commandAttribute.NamedArguments.FirstOrDefault(a => a.Key == "Description").Value.Value
            as string;

        var parameterDescriptors = new List<CommandParameterDescriptor>();
        var optionDescriptors = new List<CommandOptionDescriptor>();
        var diagnostics = new List<Diagnostic>();

        foreach (
            var property in type.GetProperties()
                .Where(p => !p.IsStatic)
                .Select(p => new PropertyDescriptor(p))
        )
        {
            var parameterAttribute = property
                .Symbol.GetAttributes()
                .FirstOrDefault(a =>
                    SymbolEqualityComparer.Default.Equals(
                        a.AttributeClass,
                        refs.CommandParameterAttribute.Symbol
                    )
                );

            if (parameterAttribute is not null)
            {
                parameterDescriptors.Add(
                    BuildParameterDescriptor(property, parameterAttribute, diagnostics)
                );
                continue;
            }

            var optionAttribute = property
                .Symbol.GetAttributes()
                .FirstOrDefault(a =>
                    SymbolEqualityComparer.Default.Equals(
                        a.AttributeClass,
                        refs.CommandOptionAttribute.Symbol
                    )
                );

            if (optionAttribute is not null)
            {
                optionDescriptors.Add(
                    BuildOptionDescriptor(property, optionAttribute, diagnostics)
                );
            }
        }

        // CLIFX006: options must have unique names and short names
        for (var i = 0; i < optionDescriptors.Count; i++)
        {
            for (var j = i + 1; j < optionDescriptors.Count; j++)
            {
                var first = optionDescriptors[i];
                var second = optionDescriptors[j];

                if (
                    !string.IsNullOrWhiteSpace(first.Name)
                    && string.Equals(first.Name, second.Name, StringComparison.OrdinalIgnoreCase)
                )
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.OptionsMustHaveUniqueNames,
                            second.Property.Symbol.Locations.FirstOrDefault() ?? Location.None,
                            second.Property.Name,
                            type.Name,
                            "name",
                            second.Name,
                            first.Property.Name
                        )
                    );
                }

                if (first.ShortName is not null && first.ShortName == second.ShortName)
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.OptionsMustHaveUniqueNames,
                            second.Property.Symbol.Locations.FirstOrDefault() ?? Location.None,
                            second.Property.Name,
                            type.Name,
                            "short name",
                            second.ShortName.ToString(),
                            first.Property.Name
                        )
                    );
                }
            }
        }

        // CLIFX007: parameters must have unique order values
        for (var i = 0; i < parameterDescriptors.Count; i++)
        {
            for (var j = i + 1; j < parameterDescriptors.Count; j++)
            {
                var first = parameterDescriptors[i];
                var second = parameterDescriptors[j];

                if (first.Order == second.Order)
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.ParametersMustHaveUniqueOrder,
                            second.Property.Symbol.Locations.FirstOrDefault() ?? Location.None,
                            second.Property.Name,
                            type.Name,
                            second.Order,
                            first.Property.Name
                        )
                    );
                }
            }
        }

        // CLIFX008: parameters must have unique names
        for (var i = 0; i < parameterDescriptors.Count; i++)
        {
            for (var j = i + 1; j < parameterDescriptors.Count; j++)
            {
                var first = parameterDescriptors[i];
                var second = parameterDescriptors[j];

                if (string.Equals(first.Name, second.Name, StringComparison.OrdinalIgnoreCase))
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.ParametersMustHaveUniqueNames,
                            second.Property.Symbol.Locations.FirstOrDefault() ?? Location.None,
                            second.Property.Name,
                            type.Name,
                            second.Name,
                            first.Property.Name
                        )
                    );
                }
            }
        }

        return new CommandDescriptor(
            new TypeDescriptor(type),
            commandName,
            commandDescription,
            parameterDescriptors,
            optionDescriptors,
            diagnostics
        );
    }

    private static CommandParameterDescriptor BuildParameterDescriptor(
        PropertyDescriptor property,
        AttributeData attribute,
        List<Diagnostic> diagnostics
    )
    {
        var order = attribute
            .ConstructorArguments.Where(a => a.Type?.SpecialType == SpecialType.System_Int32)
            .Select(a => (int)(a.Value ?? 0))
            .FirstOrDefault();
        var explicitName =
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "Name").Value.Value as string;
        var name = explicitName ?? property.Name.ToLowerInvariant();

        // CLIFX005: parameter name must not be empty
        if (explicitName is not null && string.IsNullOrWhiteSpace(explicitName))
        {
            diagnostics.Add(
                Diagnostic.Create(
                    DiagnosticDescriptors.ParameterMustHaveName,
                    property.Symbol.Locations.FirstOrDefault() ?? Location.None,
                    property.Name
                )
            );
        }

        return new CommandParameterDescriptor(
            property,
            order,
            name,
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "Description").Value.Value
                as string,
            (
                attribute.NamedArguments.FirstOrDefault(a => a.Key == "Converter").Value.Value
                as ITypeSymbol
            )
                is { } converterSym
                ? new TypeDescriptor(converterSym)
                : null,
            attribute
                .NamedArguments.Where(a => a.Key == "Validators")
                .SelectMany(a => a.Value.Values)
                .Select(v => v.Value as ITypeSymbol)
                .WhereNotNull()
                .ToArray()
                .Select(s => new TypeDescriptor(s))
                .ToArray()
        );
    }

    private static CommandOptionDescriptor BuildOptionDescriptor(
        PropertyDescriptor property,
        AttributeData attribute,
        List<Diagnostic> diagnostics
    )
    {
        var name =
            attribute
                .ConstructorArguments.Where(a => a.Type?.SpecialType == SpecialType.System_String)
                .Select(a => a.Value as string)
                .FirstOrDefault()
            ?? attribute.NamedArguments.FirstOrDefault(a => a.Key == "Name").Value.Value as string;
        var shortName =
            attribute
                .ConstructorArguments.Where(a => a.Type?.SpecialType == SpecialType.System_Char)
                .Select(a => a.Value as char?)
                .FirstOrDefault()
            ?? attribute.NamedArguments.FirstOrDefault(a => a.Key == "ShortName").Value.Value
                as char?;

        // CLIFX003: option must have a name or short name
        if (string.IsNullOrWhiteSpace(name) && shortName is null)
        {
            diagnostics.Add(
                Diagnostic.Create(
                    DiagnosticDescriptors.OptionMustHaveNameOrShortName,
                    property.Symbol.Locations.FirstOrDefault() ?? Location.None,
                    property.Name
                )
            );
        }

        // CLIFX004: option name must be valid
        if (
            !string.IsNullOrWhiteSpace(name)
            && (
                name.Length < 2
                || name.StartsWith("-", StringComparison.Ordinal)
                || name.Any(char.IsWhiteSpace)
            )
        )
        {
            diagnostics.Add(
                Diagnostic.Create(
                    DiagnosticDescriptors.OptionNameInvalid,
                    property.Symbol.Locations.FirstOrDefault() ?? Location.None,
                    name,
                    property.Name
                )
            );
        }

        return new CommandOptionDescriptor(
            property,
            name,
            shortName,
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "EnvironmentVariable").Value.Value
                as string,
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "Description").Value.Value
                as string,
            (
                attribute.NamedArguments.FirstOrDefault(a => a.Key == "Converter").Value.Value
                as ITypeSymbol
            )
                is { } converterSym
                ? new TypeDescriptor(converterSym)
                : null,
            attribute
                .NamedArguments.Where(a => a.Key == "Validators")
                .SelectMany(a => a.Value.Values)
                .Select(v => v.Value as ITypeSymbol)
                .WhereNotNull()
                .ToArray()
                .Select(s => new TypeDescriptor(s))
                .ToArray()
        );
    }

    private static void Execute(
        SourceProductionContext ctx,
        IReadOnlyList<CommandDescriptor> commands,
        CliFxReferences refs
    )
    {
        if (commands.Count == 0)
            return;

        foreach (var command in commands)
        {
            foreach (var diagnostic in command.Diagnostics)
                ctx.ReportDiagnostic(diagnostic);

            var source = GenerateCommandSchemaSource(command, refs);
            var hintName = $"{command.Type.FullyQualifiedName.Replace('.', '_')}_Schema.g.cs";
            ctx.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string GenerateCommandSchemaSource(
        CommandDescriptor command,
        CliFxReferences refs
    )
    {
        var commandFqn = command.Type.FullyQualifiedName;

        var containingNamespace = command.Type.Symbol.ContainingNamespace;
        var namespaceName =
            containingNamespace is not null && !containingNamespace.IsGlobalNamespace
                ? containingNamespace.ToDisplayString()
                : null;

        // Collect containing types from outermost to innermost (for nested class support)
        var containingTypes = new List<INamedTypeSymbol>();
        var parent = command.Type.Symbol.ContainingType;
        while (parent is not null)
        {
            containingTypes.Insert(0, parent);
            parent = parent.ContainingType;
        }

        var bindingNamespace = refs.IBindingValidator.Symbol.ContainingNamespace.ToDisplayString();
        var schemaNamespace = refs.CommandSchema.Symbol.ContainingNamespace.ToDisplayString();

        // Every command gets --help unless the user already defined it
        var needsHelpOption = !command.Options.Any(o =>
            string.Equals(o.Name, "help", StringComparison.OrdinalIgnoreCase) || o.ShortName == 'h'
        );

        // Only default commands get --version unless the user already defined it
        var needsVersionOption =
            string.IsNullOrWhiteSpace(command.Name)
            && !command.Options.Any(o =>
                string.Equals(o.Name, "version", StringComparison.OrdinalIgnoreCase)
            );

        var interfaces = new List<string>();
        if (needsHelpOption)
            interfaces.Add(refs.IHasHelpOption.ToString());
        if (needsVersionOption)
            interfaces.Add(refs.IHasVersionOption.ToString());

        var interfaceList =
            interfaces.Count > 0 ? " : " + string.Join(", ", interfaces) : string.Empty;

        var sb = new StringBuilder();

        sb.Append(
            $$"""
            // <auto-generated />
            using {{bindingNamespace}};
            using {{schemaNamespace}};

            """
        );

        if (namespaceName is not null)
        {
            sb.Append(
                $$"""
                namespace {{namespaceName}};

                """
            );
        }

        foreach (var containingType in containingTypes)
        {
            sb.AppendLine(
                $"partial class {containingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}"
            );
            sb.AppendLine("{");
        }

        sb.Append(
            $$"""
            partial class {{command.Type.Name}}{{interfaceList}}
            {
            """
        );

        if (needsHelpOption)
        {
            sb.Append(
                """

                    /// <summary>Whether the user requested help (via the -h|--help option).</summary>
                    public bool IsHelpRequested { get; set; }

                """
            );
        }

        if (needsVersionOption)
        {
            sb.Append(
                """

                    /// <summary>Whether the user requested version information (via the --version option).</summary>
                    public bool IsVersionRequested { get; set; }

                """
            );
        }

        sb.Append(
            $$"""

                /// <summary>Generated command schema for <see cref="{{command.Type.Name}}"/>.</summary>
                public static {{refs.CommandSchema}} Schema { get; } =
                    new {{refs.CommandSchema}}<{{commandFqn}}>(
                        {{EscapeString(command.Name)}},
                        {{EscapeString(command.Description)}},
                        new {{refs.CommandInputSchema}}[]
                        {
            """
        );

        foreach (var param in command.Parameters.OrderBy(p => p.Order))
            AppendParameterEntry(sb, commandFqn, param, refs);

        foreach (var option in command.Options)
            AppendOptionEntry(sb, commandFqn, option, refs);

        if (needsHelpOption)
            AppendHelpOptionEntry(sb, commandFqn, refs);

        if (needsVersionOption)
            AppendVersionOptionEntry(sb, commandFqn, refs);

        sb.Append(
            """
                        });
                }
            """
        );

        foreach (var _ in containingTypes)
        {
            sb.AppendLine();
            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    private static void AppendParameterEntry(
        StringBuilder sb,
        string commandFqn,
        CommandParameterDescriptor param,
        CliFxReferences refs
    )
    {
        var propertyTypeFqn = param.Property.Type.FullyQualifiedName;
        var (converterArg, sequenceConverterArg) = param.Property.IsSequenceType
            ? ("null", BuildSequenceConverterExpr(param.ConverterType, param.Property.Symbol, refs))
            : (GetConverterExpression(param.ConverterType, param.Property.Symbol, refs), "null");

        sb.Append(
            // lang=csharp
            $$"""
                        new {{refs.CommandParameterSchema}}<{{commandFqn}}, {{propertyTypeFqn}}>(
                            new {{refs.PropertyBinding}}<{{commandFqn}}, {{propertyTypeFqn}}>(
                                "{{param.Property.Name}}",
                                c => c.{{param.Property.Name}},
                                (c, v) => c.{{param.Property.Name}} = v),
                            {{(param.Property.IsSequenceType ? "true" : "false")}},
                            {{param.Order}},
                            {{EscapeString(param.Name)}},
                            {{(param.Property.IsRequired ? "true" : "false")}},
                            {{EscapeString(param.Description)}},
                            {{converterArg}},
                            {{sequenceConverterArg}},
                            {{BuildValidatorsExpr(param.ValidatorTypes, refs)}}),

            """
        );
    }

    private static void AppendOptionEntry(
        StringBuilder sb,
        string commandFqn,
        CommandOptionDescriptor option,
        CliFxReferences refs
    )
    {
        var propertyTypeFqn = option.Property.Type.FullyQualifiedName;
        var shortNameArg = option.ShortName.HasValue ? $"'{option.ShortName}'" : "null";
        var (converterArg, sequenceConverterArg) = option.Property.IsSequenceType
            ? (
                "null",
                BuildSequenceConverterExpr(option.ConverterType, option.Property.Symbol, refs)
            )
            : (GetConverterExpression(option.ConverterType, option.Property.Symbol, refs), "null");

        sb.Append(
            // lang=csharp
            $$"""
                        new {{refs.CommandOptionSchema}}<{{commandFqn}}, {{propertyTypeFqn}}>(
                            new {{refs.PropertyBinding}}<{{commandFqn}}, {{propertyTypeFqn}}>(
                                "{{option.Property.Name}}",
                                c => c.{{option.Property.Name}},
                                (c, v) => c.{{option.Property.Name}} = v),
                            {{(option.Property.IsSequenceType ? "true" : "false")}},
                            {{EscapeString(option.Name)}},
                            {{shortNameArg}},
                            {{EscapeString(option.EnvironmentVariable)}},
                            {{(option.Property.IsRequired ? "true" : "false")}},
                            {{EscapeString(option.Description)}},
                            {{converterArg}},
                            {{sequenceConverterArg}},
                            {{BuildValidatorsExpr(option.ValidatorTypes, refs)}}),

            """
        );
    }

    private static void AppendHelpOptionEntry(
        StringBuilder sb,
        string commandFqn,
        CliFxReferences refs
    )
    {
        sb.Append(
            // lang=csharp
            $$"""
                        new {{refs.CommandOptionSchema}}<{{commandFqn}}, bool>(
                            new {{refs.PropertyBinding}}<{{commandFqn}}, bool>(
                                "IsHelpRequested",
                                c => c.IsHelpRequested,
                                (c, v) => c.IsHelpRequested = v),
                            false,
                            "help",
                            'h',
                            null,
                            false,
                            "Shows help text.",
                            new {{refs.BoolBindingConverter}}(),
                            null,
                            global::System.Array.Empty<{{refs.IBindingValidator}}>()),

            """
        );
    }

    private static void AppendVersionOptionEntry(
        StringBuilder sb,
        string commandFqn,
        CliFxReferences refs
    )
    {
        sb.Append(
            // lang=csharp
            $$"""
                        new {{refs.CommandOptionSchema}}<{{commandFqn}}, bool>(
                            new {{refs.PropertyBinding}}<{{commandFqn}}, bool>(
                                "IsVersionRequested",
                                c => c.IsVersionRequested,
                                (c, v) => c.IsVersionRequested = v),
                            false,
                            "version",
                            null,
                            null,
                            false,
                            "Shows version information.",
                            new {{refs.BoolBindingConverter}}(),
                            null,
                            global::System.Array.Empty<{{refs.IBindingValidator}}>()),

            """
        );
    }

    private static string EscapeString(string? value) =>
        value is null ? "null" : $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";

    private static string GetConverterExpression(
        TypeDescriptor? converterType,
        IPropertySymbol property,
        CliFxReferences refs
    )
    {
        if (converterType is not null)
            return $"new {converterType.GlobalFullyQualifiedName}()";

        var type = property.Type;

        // For sequence types, use the element type's converter
        if (
            type.SpecialType != SpecialType.System_String
            && type.TryGetEnumerableUnderlyingType() is { } elementType
        )
            return BuildDefaultConverterExprForScalar(elementType, refs) ?? "null";

        return BuildDefaultConverterExprForScalar(type, refs) ?? "null";
    }

    private static string BuildValidatorsExpr(
        IReadOnlyList<TypeDescriptor> validatorTypes,
        CliFxReferences refs
    )
    {
        if (validatorTypes.Count == 0)
            return $"global::System.Array.Empty<{refs.IBindingValidator}>()";

        var items = string.Join(
            ", ",
            validatorTypes.Select(v => $"new {v.GlobalFullyQualifiedName}()")
        );
        return $"new {refs.IBindingValidator}[] {{ {items} }}";
    }

    private static string? BuildDefaultConverterExprForScalar(
        ITypeSymbol type,
        CliFxReferences refs
    )
    {
        var typeFqn = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // string — no conversion needed (null converter = pass-through)
        if (type.SpecialType == SpecialType.System_String)
            return null;

        // object — assignable from string; null converter passes raw string through as object
        if (type.SpecialType == SpecialType.System_Object)
            return null;

        // bool
        if (type.SpecialType == SpecialType.System_Boolean)
            return $"new {refs.BoolBindingConverter}()";

        // DateTimeOffset
        if (type is INamedTypeSymbol { ContainingNamespace.Name: "System", Name: "DateTimeOffset" })
            return $"new {refs.DateTimeOffsetBindingConverter}()";

        // TimeSpan
        if (type is INamedTypeSymbol { ContainingNamespace.Name: "System", Name: "TimeSpan" })
            return $"new {refs.TimeSpanBindingConverter}()";

        // Enum
        if (type.TypeKind == TypeKind.Enum)
            return $"new {refs.EnumBindingConverter.GlobalBaseFullyQualifiedName}<{typeFqn}>()";

        // Nullable<T>
        if (
            type is INamedTypeSymbol { IsValueType: true } named
            && named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T
        )
        {
            var innerType = named.TypeArguments[0];
            var innerFqn = innerType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var innerConverterExpr = BuildDefaultConverterExprForScalar(innerType, refs);
            if (innerConverterExpr is null)
                return null;
            return $"new {refs.NullableBindingConverter.GlobalBaseFullyQualifiedName}<{innerFqn}>({innerConverterExpr})";
        }

        // IConvertible (int, double, char, etc.)
        if (
            type.AllInterfaces.Any(i =>
                i.ContainingNamespace?.Name == "System" && i.Name == "IConvertible"
            )
        )
            return $"new {refs.ConvertibleBindingConverter.GlobalBaseFullyQualifiedName}<{typeFqn}>()";

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
            return $"new {refs.DelegateBindingConverter.GlobalBaseFullyQualifiedName}<{typeFqn}>(s => {typeFqn}.Parse(s!, global::System.Globalization.CultureInfo.InvariantCulture))";

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
            return $"new {refs.DelegateBindingConverter.GlobalBaseFullyQualifiedName}<{typeFqn}>(s => {typeFqn}.Parse(s!))";

        // String-constructable: public constructor(string)
        if (
            type is INamedTypeSymbol namedConstructable
            && namedConstructable.Constructors.Any(c =>
                c.DeclaredAccessibility == Accessibility.Public
                && c.Parameters.Length == 1
                && c.Parameters[0].Type.SpecialType == SpecialType.System_String
            )
        )
            return $"new {refs.DelegateBindingConverter.GlobalBaseFullyQualifiedName}<{typeFqn}>(s => new {typeFqn}(s!))";

        return null;
    }

    // Builds a collection converter expression for a sequence property.
    // Returns "null" if the collection type is not supported for AOT-compatible generation.
    private static string BuildSequenceConverterExpr(
        TypeDescriptor? userConverterType,
        IPropertySymbol property,
        CliFxReferences refs
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

        var elementConverterArg = userConverterType is not null
            ? $"new {userConverterType.GlobalFullyQualifiedName}()"
            : BuildDefaultConverterExprForScalar(elementType, refs) ?? "null";

        // T[] — use the single-type-parameter convenience form
        if (collectionType is IArrayTypeSymbol)
            return $"new {refs.ArraySequenceBindingConverter.GlobalBaseFullyQualifiedName}<{elementTypeFqn}>({elementConverterArg})";

        // Interface (IReadOnlyList<T>, IEnumerable<T>, etc.) — T[] is assignable to any of these;
        // use the two-type-parameter form so the result type matches TSequence directly.
        if (collectionType.TypeKind == TypeKind.Interface)
            return $"new {refs.ArraySequenceBindingConverter.GlobalBaseFullyQualifiedName}<{elementTypeFqn}, {collectionTypeFqn}>({elementConverterArg})";

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
            return $"new {refs.ArrayInitializableSequenceBindingConverter.GlobalBaseFullyQualifiedName}<{elementTypeFqn}, {collectionTypeFqn}>({elementConverterArg}, arr => new {collectionTypeFqn}(arr))";
        }

        // Unknown collection type — user must provide a custom ISequenceBindingConverter
        return "null";
    }
}
