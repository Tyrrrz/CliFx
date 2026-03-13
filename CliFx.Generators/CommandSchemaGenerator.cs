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

        var commandNodesWithWkt = commandNodes.Combine(cliFxRefs);

        // Emit diagnostics for [Command] classes that are not partial or don't implement ICommand
        var diagnostics = commandNodesWithWkt.SelectMany(
            static (pair, _) =>
            {
                var (item, cliFxRefs) = pair;

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
                        SymbolEqualityComparer.Default.Equals(i, cliFxRefs.ICommand.Symbol)
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
        var commandDeclarations = commandNodesWithWkt
            .Select(
                static (pair, _) =>
                {
                    var (item, wkt) = pair;
                    if (
                        !item.ClassDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
                        || item.Symbol.IsAbstract
                        || !item.Symbol.AllInterfaces.Any(i =>
                            SymbolEqualityComparer.Default.Equals(i, wkt.ICommand.Symbol)
                        )
                    )
                        return null;

                    return TryBuildCommandDescriptor(item.Symbol, wkt);
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
        CliFxReferences wkt
    )
    {
        // Must implement ICommand
        if (
            !type.AllInterfaces.Any(i =>
                SymbolEqualityComparer.Default.Equals(i, wkt.ICommand.Symbol)
            )
        )
            return null;

        // Must have [Command] attribute
        var commandAttribute = type.GetAttributes()
            .FirstOrDefault(a =>
                SymbolEqualityComparer.Default.Equals(a.AttributeClass, wkt.CommandAttribute.Symbol)
            );

        if (commandAttribute is null)
            return null;

        // Must be a concrete class
        if (type.IsAbstract)
            return null;

        var commandName =
            commandAttribute
                .NamedArguments.Where(a => a.Key == "Name")
                .Select(a => a.Value.Value as string)
                .FirstOrDefault()
            ?? commandAttribute
                .ConstructorArguments.Where(a => a.Type?.SpecialType == SpecialType.System_String)
                .Select(a => a.Value as string)
                .FirstOrDefault();

        var commandDescription = commandAttribute
            .NamedArguments.Where(a => a.Key == "Description")
            .Select(a => a.Value.Value as string)
            .FirstOrDefault();

        var parameterDescriptors = new List<CommandParameterDescriptor>();
        var optionDescriptors = new List<CommandOptionDescriptor>();
        var diagnostics = new List<Diagnostic>();

        foreach (
            var propertyDescriptor in type.GetProperties()
                .Where(p => !p.IsStatic)
                .Select(p => new PropertyDescriptor(p))
        )
        {
            var parameterAttribute = propertyDescriptor
                .Symbol.GetAttributes()
                .FirstOrDefault(a =>
                    SymbolEqualityComparer.Default.Equals(
                        a.AttributeClass,
                        wkt.CommandParameterAttribute.Symbol
                    )
                );

            if (parameterAttribute is not null)
            {
                var order = parameterAttribute
                    .ConstructorArguments.Where(a =>
                        a.Type?.SpecialType == SpecialType.System_Int32
                    )
                    .Select(a => (int)(a.Value ?? 0))
                    .FirstOrDefault();

                var name =
                    parameterAttribute
                        .NamedArguments.Where(a => a.Key == "Name")
                        .Select(a => a.Value.Value as string)
                        .FirstOrDefault()
                    ?? propertyDescriptor.Name.ToLowerInvariant();

                // CLIFX005: parameter name must not be empty
                var explicitName = parameterAttribute
                    .NamedArguments.Where(a => a.Key == "Name")
                    .Select(a => a.Value.Value as string)
                    .FirstOrDefault();

                if (explicitName is not null && string.IsNullOrWhiteSpace(explicitName))
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.ParameterMustHaveName,
                            propertyDescriptor.Symbol.Locations.FirstOrDefault() ?? Location.None,
                            propertyDescriptor.Name
                        )
                    );
                }

                var description = parameterAttribute
                    .NamedArguments.Where(a => a.Key == "Description")
                    .Select(a => a.Value.Value as string)
                    .FirstOrDefault();

                var converterTypeDescriptor = parameterAttribute
                    .NamedArguments.Where(a => a.Key == "Converter")
                    .Select(a => a.Value.Value is ITypeSymbol sym ? new TypeDescriptor(sym) : null)
                    .FirstOrDefault();

                var validatorTypeDescriptors = parameterAttribute
                    .NamedArguments.Where(a => a.Key == "Validators")
                    .SelectMany(a => a.Value.Values)
                    .Select(v => v.Value is ITypeSymbol sym ? new TypeDescriptor(sym) : null)
                    .Where(t => t is not null)
                    .Select(t => t!)
                    .ToArray();

                parameterDescriptors.Add(
                    new CommandParameterDescriptor(
                        propertyDescriptor,
                        order,
                        name,
                        description,
                        converterTypeDescriptor,
                        validatorTypeDescriptors
                    )
                );

                continue;
            }

            var optionAttribute = propertyDescriptor
                .Symbol.GetAttributes()
                .FirstOrDefault(a =>
                    SymbolEqualityComparer.Default.Equals(
                        a.AttributeClass,
                        wkt.CommandOptionAttribute.Symbol
                    )
                );

            if (optionAttribute is not null)
            {
                var name =
                    optionAttribute
                        .ConstructorArguments.Where(a =>
                            a.Type?.SpecialType == SpecialType.System_String
                        )
                        .Select(a => a.Value as string)
                        .FirstOrDefault()
                    ?? optionAttribute
                        .NamedArguments.Where(a => a.Key == "Name")
                        .Select(a => a.Value.Value as string)
                        .FirstOrDefault();

                var shortName =
                    optionAttribute
                        .ConstructorArguments.Where(a =>
                            a.Type?.SpecialType == SpecialType.System_Char
                        )
                        .Select(a => a.Value as char?)
                        .FirstOrDefault()
                    ?? optionAttribute
                        .NamedArguments.Where(a => a.Key == "ShortName")
                        .Select(a => a.Value.Value as char?)
                        .FirstOrDefault();

                // CLIFX003: option must have a name or short name
                if (string.IsNullOrWhiteSpace(name) && shortName is null)
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.OptionMustHaveNameOrShortName,
                            propertyDescriptor.Symbol.Locations.FirstOrDefault() ?? Location.None,
                            propertyDescriptor.Name
                        )
                    );
                }

                // CLIFX004: option name must be valid
                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (
                        name.Length < 2
                        || name.StartsWith("-", StringComparison.Ordinal)
                        || name.Any(char.IsWhiteSpace)
                    )
                    {
                        diagnostics.Add(
                            Diagnostic.Create(
                                DiagnosticDescriptors.OptionNameInvalid,
                                propertyDescriptor.Symbol.Locations.FirstOrDefault()
                                    ?? Location.None,
                                name,
                                propertyDescriptor.Name
                            )
                        );
                    }
                }

                var description = optionAttribute
                    .NamedArguments.Where(a => a.Key == "Description")
                    .Select(a => a.Value.Value as string)
                    .FirstOrDefault();

                var environmentVariable = optionAttribute
                    .NamedArguments.Where(a => a.Key == "EnvironmentVariable")
                    .Select(a => a.Value.Value as string)
                    .FirstOrDefault();

                var converterTypeDescriptor = optionAttribute
                    .NamedArguments.Where(a => a.Key == "Converter")
                    .Select(a => a.Value.Value is ITypeSymbol sym ? new TypeDescriptor(sym) : null)
                    .FirstOrDefault();

                var validatorTypeDescriptors = optionAttribute
                    .NamedArguments.Where(a => a.Key == "Validators")
                    .SelectMany(a => a.Value.Values)
                    .Select(v => v.Value is ITypeSymbol sym ? new TypeDescriptor(sym) : null)
                    .Where(t => t is not null)
                    .Select(t => t!)
                    .ToArray();

                optionDescriptors.Add(
                    new CommandOptionDescriptor(
                        propertyDescriptor,
                        name,
                        shortName,
                        environmentVariable,
                        description,
                        converterTypeDescriptor,
                        validatorTypeDescriptors
                    )
                );
            }
        }

        // CLIFX006: options must have unique names and short names
        for (var i = 0; i < optionDescriptors.Count; i++)
        {
            for (var j = i + 1; j < optionDescriptors.Count; j++)
            {
                var a = optionDescriptors[i];
                var b = optionDescriptors[j];

                if (
                    !string.IsNullOrWhiteSpace(a.Name)
                    && string.Equals(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)
                )
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.OptionsMustHaveUniqueNames,
                            b.Property.Symbol.Locations.FirstOrDefault() ?? Location.None,
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
                            b.Property.Symbol.Locations.FirstOrDefault() ?? Location.None,
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
        for (var i = 0; i < parameterDescriptors.Count; i++)
        {
            for (var j = i + 1; j < parameterDescriptors.Count; j++)
            {
                var a = parameterDescriptors[i];
                var b = parameterDescriptors[j];

                if (a.Order == b.Order)
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.ParametersMustHaveUniqueOrder,
                            b.Property.Symbol.Locations.FirstOrDefault() ?? Location.None,
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
        for (var i = 0; i < parameterDescriptors.Count; i++)
        {
            for (var j = i + 1; j < parameterDescriptors.Count; j++)
            {
                var a = parameterDescriptors[i];
                var b = parameterDescriptors[j];

                if (string.Equals(a.Name, b.Name, StringComparison.OrdinalIgnoreCase))
                {
                    diagnostics.Add(
                        Diagnostic.Create(
                            DiagnosticDescriptors.ParametersMustHaveUniqueNames,
                            b.Property.Symbol.Locations.FirstOrDefault() ?? Location.None,
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
            new TypeDescriptor(type),
            commandName,
            commandDescription,
            parameterDescriptors,
            optionDescriptors,
            diagnostics
        );
    }

    private static void Execute(
        SourceProductionContext ctx,
        IReadOnlyList<CommandDescriptor> commands,
        CliFxReferences wkt
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

            var source = GenerateCommandSchemaSource(command, wkt);
            var hintName = $"{command.Type.FullyQualifiedName.Replace('.', '_')}_Schema.g.cs";
            ctx.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string GenerateCommandSchemaSource(
        CommandDescriptor command,
        CliFxReferences wkt
    )
    {
        var commandFqn = command.Type.FullyQualifiedName;
        var containingNs = command.Type.Symbol.ContainingNamespace;
        var ns =
            containingNs is not null && !containingNs.IsGlobalNamespace
                ? containingNs.ToDisplayString()
                : null;

        // Collect containing types from outermost to innermost (for nested class support)
        var containingTypes = new List<INamedTypeSymbol>();
        var ct = command.Type.Symbol.ContainingType;
        while (ct is not null)
        {
            containingTypes.Insert(0, ct);
            ct = ct.ContainingType;
        }

        // Namespace strings for the using directives in the emitted file.
        // All CliFx type FQNs are emitted directly via wkt.TypeName
        // (TypeDescriptor.ToString() returns the global:: form).
        var nsBinding = wkt.IBindingValidator.Symbol.ContainingNamespace.ToDisplayString();
        var nsSchema = wkt.CommandSchema.Symbol.ContainingNamespace.ToDisplayString();

        var sb = new StringBuilder();

        sb.Append(
            // lang=csharp
            $$"""
            // <auto-generated/>
            using {{nsBinding}};
            using {{nsSchema}};

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
            interfaces.Add(wkt.IHasHelpOption.ToString());

        if (needsVersionOption)
            interfaces.Add(wkt.IHasVersionOption.ToString());

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
                public static {{wkt.CommandSchema}} Schema { get; } =
                    new {{wkt.CommandSchema}}<{{commandFqn}}>(
                        {{EscapeString(command.Name)}},
                        {{EscapeString(command.Description)}},
                        new {{wkt.CommandInputSchema}}[]
                        {
            """
        );

        foreach (var param in command.Parameters.OrderBy(p => p.Order))
        {
            var propTypeFqn = param.Property.Type.FullyQualifiedName;
            var isSequence =
                param.Property.Type.Symbol.SpecialType != SpecialType.System_String
                && param.Property.Type.Symbol.TryGetEnumerableUnderlyingType() is not null;
            var converterExpr = GetConverterExpression(
                param.ConverterType,
                param.Property.Symbol,
                wkt
            );

            if (!isSequence)
            {
                sb.Append(
                    // lang=csharp
                    $$"""
                                new {{wkt.CommandParameterSchema}}<{{commandFqn}}, {{propTypeFqn}}>(
                                    new {{wkt.PropertyBinding}}<{{commandFqn}}, {{propTypeFqn}}>(
                                        "{{param.Property.Name}}",
                                        c => c.{{param.Property.Name}},
                                        (c, v) => c.{{param.Property.Name}} = v),
                                    false,
                                    {{param.Order}},
                                    {{EscapeString(param.Name)}},
                                    {{(param.Property.IsRequired ? "true" : "false")}},
                                    {{EscapeString(param.Description)}},
                                    {{converterExpr}},
                                    null,
                                    {{BuildValidatorsExpr(param.ValidatorTypes, wkt)}}),

                    """
                );
            }
            else
            {
                var sequenceConverterExpr = BuildSequenceConverterExpr(
                    param.ConverterType,
                    param.Property.Symbol,
                    wkt
                );
                sb.Append(
                    // lang=csharp
                    $$"""
                                new {{wkt.CommandParameterSchema}}<{{commandFqn}}, {{propTypeFqn}}>(
                                    new {{wkt.PropertyBinding}}<{{commandFqn}}, {{propTypeFqn}}>(
                                        "{{param.Property.Name}}",
                                        c => c.{{param.Property.Name}},
                                        (c, v) => c.{{param.Property.Name}} = v),
                                    true,
                                    {{param.Order}},
                                    {{EscapeString(param.Name)}},
                                    {{(param.Property.IsRequired ? "true" : "false")}},
                                    {{EscapeString(param.Description)}},
                                    null,
                                    {{sequenceConverterExpr}},
                                    {{BuildValidatorsExpr(param.ValidatorTypes, wkt)}}),

                    """
                );
            }
        }

        foreach (var opt in command.Options)
        {
            var propTypeFqn = opt.Property.Type.FullyQualifiedName;
            var isSequence =
                opt.Property.Type.Symbol.SpecialType != SpecialType.System_String
                && opt.Property.Type.Symbol.TryGetEnumerableUnderlyingType() is not null;
            var shortNameStr = opt.ShortName.HasValue ? $"'{opt.ShortName}'" : "null";
            var converterExpr = GetConverterExpression(opt.ConverterType, opt.Property.Symbol, wkt);

            if (!isSequence)
            {
                sb.Append(
                    // lang=csharp
                    $$"""
                                new {{wkt.CommandOptionSchema}}<{{commandFqn}}, {{propTypeFqn}}>(
                                    new {{wkt.PropertyBinding}}<{{commandFqn}}, {{propTypeFqn}}>(
                                        "{{opt.Property.Name}}",
                                        c => c.{{opt.Property.Name}},
                                        (c, v) => c.{{opt.Property.Name}} = v),
                                    false,
                                    {{EscapeString(opt.Name)}},
                                    {{shortNameStr}},
                                    {{EscapeString(opt.EnvironmentVariable)}},
                                    {{(opt.Property.IsRequired ? "true" : "false")}},
                                    {{EscapeString(opt.Description)}},
                                    {{converterExpr}},
                                    null,
                                    {{BuildValidatorsExpr(opt.ValidatorTypes, wkt)}}),

                    """
                );
            }
            else
            {
                var sequenceConverterExpr = BuildSequenceConverterExpr(
                    opt.ConverterType,
                    opt.Property.Symbol,
                    wkt
                );
                sb.Append(
                    // lang=csharp
                    $$"""
                                new {{wkt.CommandOptionSchema}}<{{commandFqn}}, {{propTypeFqn}}>(
                                    new {{wkt.PropertyBinding}}<{{commandFqn}}, {{propTypeFqn}}>(
                                        "{{opt.Property.Name}}",
                                        c => c.{{opt.Property.Name}},
                                        (c, v) => c.{{opt.Property.Name}} = v),
                                    true,
                                    {{EscapeString(opt.Name)}},
                                    {{shortNameStr}},
                                    {{EscapeString(opt.EnvironmentVariable)}},
                                    {{(opt.Property.IsRequired ? "true" : "false")}},
                                    {{EscapeString(opt.Description)}},
                                    null,
                                    {{sequenceConverterExpr}},
                                    {{BuildValidatorsExpr(opt.ValidatorTypes, wkt)}}),

                    """
                );
            }
        }

        if (needsHelpOption)
        {
            sb.Append(
                // lang=csharp
                $$"""
                            new {{wkt.CommandOptionSchema}}<{{commandFqn}}, bool>(
                                new {{wkt.PropertyBinding}}<{{commandFqn}}, bool>(
                                    "IsHelpRequested",
                                    c => c.IsHelpRequested,
                                    (c, v) => c.IsHelpRequested = v),
                                false,
                                "help",
                                'h',
                                null,
                                false,
                                "Shows help text.",
                                new {{wkt.BoolBindingConverter}}(),
                                null,
                                global::System.Array.Empty<{{wkt.IBindingValidator}}>()),

                """
            );
        }

        if (needsVersionOption)
        {
            sb.Append(
                // lang=csharp
                $$"""
                            new {{wkt.CommandOptionSchema}}<{{commandFqn}}, bool>(
                                new {{wkt.PropertyBinding}}<{{commandFqn}}, bool>(
                                    "IsVersionRequested",
                                    c => c.IsVersionRequested,
                                    (c, v) => c.IsVersionRequested = v),
                                false,
                                "version",
                                null,
                                null,
                                false,
                                "Shows version information.",
                                new {{wkt.BoolBindingConverter}}(),
                                null,
                                global::System.Array.Empty<{{wkt.IBindingValidator}}>()),

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

    private static string EscapeString(string? value) =>
        value is null ? "null" : $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";

    private static string GetConverterExpression(
        TypeDescriptor? converterType,
        IPropertySymbol property,
        CliFxReferences wkt
    )
    {
        if (converterType is not null)
            return $"new {converterType.GlobalFullyQualifiedName}()";

        var type = property.Type;

        // For sequence types, use the element type's converter
        if (
            type.SpecialType != SpecialType.System_String
            && type.TryGetEnumerableUnderlyingType() is not null
        )
        {
            var elementType = type.TryGetEnumerableUnderlyingType();
            if (elementType is not null)
                return BuildDefaultConverterExprForScalar(elementType, wkt) ?? "null";
        }

        return BuildDefaultConverterExprForScalar(type, wkt) ?? "null";
    }

    private static string BuildValidatorsExpr(
        IReadOnlyList<TypeDescriptor> validatorTypes,
        CliFxReferences wkt
    )
    {
        if (validatorTypes.Count == 0)
            return $"global::System.Array.Empty<{wkt.IBindingValidator}>()";

        var items = string.Join(
            ", ",
            validatorTypes.Select(v => $"new {v.GlobalFullyQualifiedName}()")
        );
        return $"new {wkt.IBindingValidator}[] {{ {items} }}";
    }

    private static string? BuildDefaultConverterExprForScalar(ITypeSymbol type, CliFxReferences wkt)
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
            return $"new {wkt.BoolBindingConverter}()";

        // DateTimeOffset
        if (type is INamedTypeSymbol { ContainingNamespace.Name: "System", Name: "DateTimeOffset" })
            return $"new {wkt.DateTimeOffsetBindingConverter}()";

        // TimeSpan
        if (type is INamedTypeSymbol { ContainingNamespace.Name: "System", Name: "TimeSpan" })
            return $"new {wkt.TimeSpanBindingConverter}()";

        // Enum
        if (type.TypeKind == TypeKind.Enum)
            return $"new {wkt.EnumBindingConverter.GlobalBase}<{fqn}>()";

        // Nullable<T>
        if (
            type is INamedTypeSymbol { IsValueType: true } named
            && named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T
        )
        {
            var innerType = named.TypeArguments[0];
            var innerFqn = innerType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var innerConverterExpr = BuildDefaultConverterExprForScalar(innerType, wkt);
            if (innerConverterExpr is null)
                return null;
            return $"new {wkt.NullableBindingConverter.GlobalBase}<{innerFqn}>({innerConverterExpr})";
        }

        // IConvertible (int, double, char, etc.)
        if (
            type.AllInterfaces.Any(i =>
                i.ContainingNamespace?.Name == "System" && i.Name == "IConvertible"
            )
        )
            return $"new {wkt.ConvertibleBindingConverter.GlobalBase}<{fqn}>()";

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
            return $"new {wkt.DelegateBindingConverter.GlobalBase}<{fqn}>(s => {fqn}.Parse(s!, global::System.Globalization.CultureInfo.InvariantCulture))";

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
            return $"new {wkt.DelegateBindingConverter.GlobalBase}<{fqn}>(s => {fqn}.Parse(s!))";

        // String-constructable: public constructor(string)
        if (
            type is INamedTypeSymbol namedConstructable
            && namedConstructable.Constructors.Any(c =>
                c.DeclaredAccessibility == Accessibility.Public
                && c.Parameters.Length == 1
                && c.Parameters[0].Type.SpecialType == SpecialType.System_String
            )
        )
            return $"new {wkt.DelegateBindingConverter.GlobalBase}<{fqn}>(s => new {fqn}(s!))";

        return null;
    }

    // Builds a collection converter expression for a sequence property.
    // Returns "null" if the collection type is not supported for AOT-compatible generation.
    private static string BuildSequenceConverterExpr(
        TypeDescriptor? userConverterType,
        IPropertySymbol property,
        CliFxReferences wkt
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
            elementConverterExpr = $"new {userConverterType.GlobalFullyQualifiedName}()";
        else
            elementConverterExpr = BuildDefaultConverterExprForScalar(elementType, wkt);

        var elementConverterArg = elementConverterExpr ?? "null";

        // T[] — use the single-type-parameter convenience form
        if (collectionType is IArrayTypeSymbol)
            return $"new {wkt.ArraySequenceBindingConverter.GlobalBase}<{elementTypeFqn}>({elementConverterArg})";

        // Interface (IReadOnlyList<T>, IEnumerable<T>, etc.) — T[] is assignable to any of these;
        // use the two-type-parameter form so the result type matches TSequence directly.
        if (collectionType.TypeKind == TypeKind.Interface)
            return $"new {wkt.ArraySequenceBindingConverter.GlobalBase}<{elementTypeFqn}, {collectionTypeFqn}>({elementConverterArg})";

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
            return $"new {wkt.ArrayInitializableSequenceBindingConverter.GlobalBase}<{elementTypeFqn}, {collectionTypeFqn}>({elementConverterArg}, arr => new {collectionTypeFqn}(arr))";
        }

        // Unknown collection type — user must provide a custom ISequenceBindingConverter
        return "null";
    }
}
