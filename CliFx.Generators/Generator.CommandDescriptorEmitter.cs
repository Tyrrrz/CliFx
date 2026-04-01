using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Generators.Binding;
using CliFx.Generators.Utils;
using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators;

internal static class CommandDescriptorEmitter
{
    private static string EmitValidators(
        IReadOnlyList<INamedTypeSymbol> validatorTypes,
        string propertyTypeName
    )
    {
        if (validatorTypes.Count == 0)
            return $"global::System.Array.Empty<global::CliFx.Activation.IInputValidator<{propertyTypeName}>>()";

        // lang=csharp
        return $$"""
                new global::CliFx.Activation.IInputValidator<{{propertyTypeName}}>[]
                {
                    {{string.Join(
                        ", ",
                        validatorTypes.Select(v => $"new {v.GetGloballyQualifiedName()}()")
                    )}}
                }
                """;
    }

    private static string? TryEmitDefaultScalarConverter(ITypeSymbol type)
    {
        // Object
        if (type.SpecialType == SpecialType.System_Object)
            return "new global::CliFx.Activation.StringScalarInputConverter()";

        // String
        if (type.SpecialType == SpecialType.System_String)
            return "new global::CliFx.Activation.StringScalarInputConverter()";

        // Bool
        if (type.SpecialType == SpecialType.System_Boolean)
            return "new global::CliFx.Activation.BoolScalarInputConverter()";

        // Enum
        if (type.TypeKind == TypeKind.Enum)
            return $"new global::CliFx.Activation.EnumScalarInputConverter<{type.GetGloballyQualifiedName()}>()";

        // Nullable<T>
        if (type.TryGetNullableUnderlyingType() is { } underlyingType)
        {
            var innerConverterCode = TryEmitDefaultScalarConverter(underlyingType);
            if (innerConverterCode is null)
                return null;

            return $"global::CliFx.Activation.NullableScalarInputConverter.Create({innerConverterCode})";
        }

        // Has static Parse(string, IFormatProvider)
        var parseMethodWithFormatProvider = type.GetMethods()
            .FirstOrDefault(m =>
                string.Equals(m.Name, "Parse", StringComparison.Ordinal)
                && m.IsStatic
                && m.DeclaredAccessibility == Accessibility.Public
                && m.Parameters.Length == 2
                && m.Parameters[0].Type.SpecialType == SpecialType.System_String
                && m.Parameters[1].Type.IsMatchedBy("System.IFormatProvider")
                && SymbolEqualityComparer.Default.Equals(m.ReturnType, type)
            );

        if (parseMethodWithFormatProvider is not null)
        {
            return $"""
                global::CliFx.Activation.DelegateScalarInputConverter.Create(
                    v => {parseMethodWithFormatProvider.GetGloballyQualifiedName()}(
                        v!,
                        global::System.Globalization.CultureInfo.InvariantCulture
                    )
                )
                """;
        }

        // Has static Parse(string)
        var parseMethod = type.GetMethods()
            .FirstOrDefault(m =>
                string.Equals(m.Name, "Parse", StringComparison.Ordinal)
                && m.IsStatic
                && m.DeclaredAccessibility == Accessibility.Public
                && m.Parameters.Length == 1
                && m.Parameters[0].Type.SpecialType == SpecialType.System_String
                && SymbolEqualityComparer.Default.Equals(m.ReturnType, type)
            );

        if (parseMethod is not null)
        {
            // lang=csharp
            return $"""
                global::CliFx.Activation.DelegateScalarInputConverter.Create(
                    v => {parseMethod.GetGloballyQualifiedName()}(v!)
                )
                """;
        }

        // Has ctor(string)
        if (
            type is INamedTypeSymbol namedConstructable
            && namedConstructable.Constructors.Any(c =>
                c.DeclaredAccessibility == Accessibility.Public
                && c.Parameters.Length == 1
                && c.Parameters[0].Type.SpecialType == SpecialType.System_String
            )
        )
        {
            // lang=csharp
            return $"""
                global::CliFx.Activation.DelegateScalarInputConverter.Create(
                    v => new {type.GetGloballyQualifiedName()}(v!)
                )
                """;
        }

        // Implements IConvertible
        if (
            type.AllInterfaces.Any(i =>
                i.ContainingNamespace?.Name == "System" && i.Name == "IConvertible"
            )
        )
        {
            // lang=csharp
            return $"""
                new global::CliFx.Activation.ConvertibleScalarInputConverter<{type.GetGloballyQualifiedName()}>()
                """;
        }

        return null;
    }

    private static string? TryEmitDefaultSequenceConverter(
        ITypeSymbol collectionType,
        ITypeSymbol elementType
    )
    {
        var elementConverterFragment = TryEmitDefaultScalarConverter(elementType);
        if (elementConverterFragment is null)
            return null;

        // Assignable from T[]
        if (
            // T[]
            collectionType is IArrayTypeSymbol
            || (
                collectionType
                    is INamedTypeSymbol
                    {
                        TypeKind: TypeKind.Interface,
                        TypeArguments: [var interfaceElementType],
                    } collectionInterfaceType
                && collectionInterfaceType.ConstructedFrom.SpecialType
                    // IEnumerable<T>
                    is SpecialType.System_Collections_Generic_IEnumerable_T
                        // ICollection<T>
                        or SpecialType.System_Collections_Generic_ICollection_T
                        // IList<T>
                        or SpecialType.System_Collections_Generic_IList_T
                        // IReadOnlyCollection<T>
                        or SpecialType.System_Collections_Generic_IReadOnlyCollection_T
                        // IReadOnlyList<T>
                        or SpecialType.System_Collections_Generic_IReadOnlyList_T
                && SymbolEqualityComparer.Default.Equals(interfaceElementType, elementType)
            )
        )
        {
            return $"global::CliFx.Activation.ArraySequenceInputConverter.Create({elementConverterFragment})";
        }

        // Has ctor(T[])
        if (
            collectionType is INamedTypeSymbol collectionNamedType
            && collectionNamedType.Constructors.Any(c =>
                c.DeclaredAccessibility == Accessibility.Public
                && c.Parameters.Length == 1
                && (
                    // T[]
                    (
                        c.Parameters[0].Type is IArrayTypeSymbol arrayParameterType
                        && SymbolEqualityComparer.Default.Equals(
                            arrayParameterType.ElementType,
                            elementType
                        )
                    )
                    // Interfaces implemented by T[]
                    || (
                        c.Parameters[0].Type is INamedTypeSymbol namedParameterType
                        && namedParameterType.ConstructedFrom.SpecialType
                            // IEnumerable<T>
                            is SpecialType.System_Collections_Generic_IEnumerable_T
                                // ICollection<T>
                                or SpecialType.System_Collections_Generic_ICollection_T
                                // IList<T>
                                or SpecialType.System_Collections_Generic_IList_T
                                // IReadOnlyCollection<T>
                                or SpecialType.System_Collections_Generic_IReadOnlyCollection_T
                                // IReadOnlyList<T>
                                or SpecialType.System_Collections_Generic_IReadOnlyList_T
                        && SymbolEqualityComparer.Default.Equals(
                            namedParameterType.TypeArguments[0],
                            elementType
                        )
                    )
                )
            )
        )
        {
            // lang=csharp
            return $"""
                global::CliFx.Activation.DelegateSequenceInputConverter.Create(
                    global::CliFx.Activation.ArraySequenceInputConverter.Create({elementConverterFragment}),
                    vs => new {collectionType.GetGloballyQualifiedName()}(vs)
                )
                """;
        }

        return null;
    }

    private static string? TryEmitConverter(
        INamedTypeSymbol? userConverterType,
        IPropertySymbol property
    )
    {
        if (userConverterType is not null)
            return $"new {userConverterType.GetGloballyQualifiedName()}()";

        // Sequence-based property
        if (
            property.Type.SpecialType != SpecialType.System_String
            && property.Type.TryGetEnumerableUnderlyingType() is { } elementType
        )
        {
            return TryEmitDefaultSequenceConverter(property.Type, elementType);
        }

        // Scalar property
        return TryEmitDefaultScalarConverter(property.Type);
    }

    private static string? TryEmitCommandParameterDescriptor(
        CommandSymbol command,
        CommandParameterSymbol parameter,
        DiagnosticReporter diagnostics
    )
    {
        var converterFragment = TryEmitConverter(parameter.ConverterType, parameter.Property);
        if (converterFragment is null)
        {
            diagnostics.Report(
                DiagnosticDescriptors.CommandInputConverterNotInferrable,
                parameter.Property.Locations.FirstOrDefault(),
                parameter.Property.Name,
                parameter.Property.Type.GetGloballyQualifiedName()
            );

            return null;
        }

        // lang=csharp
        return $$"""
            new global::CliFx.Binding.CommandParameterDescriptor<{{command.Type.GetGloballyQualifiedName()}}, {{parameter.Property.Type.GetGloballyQualifiedName()}}>(
                new global::CliFx.Binding.PropertyDescriptor<{{command.Type.GetGloballyQualifiedName()}}, {{parameter.Property.Type.GetGloballyQualifiedName()}}>(
                    {{CSharp.Encode(parameter.Property.Name)}},
                    c => c.{{parameter.Property.Name}},
                    (c, v) => c.{{parameter.Property.Name}} = v
                ),
                {{parameter.Order}},
                {{CSharp.Encode(parameter.Name)}},
                {{parameter.IsRequired.ToString().ToLowerInvariant()}},
                {{CSharp.Encode(parameter.Description)}},
                {{converterFragment}},
                {{EmitValidators(
                parameter.ValidatorTypes,
                parameter.Property.Type.GetGloballyQualifiedName()
            )}}
            )
            """;
    }

    private static string? TryEmitCommandOptionDescriptor(
        CommandSymbol command,
        CommandOptionSymbol option,
        DiagnosticReporter diagnostics
    )
    {
        var converterFragment = TryEmitConverter(option.ConverterType, option.Property);
        if (converterFragment is null)
        {
            diagnostics.Report(
                DiagnosticDescriptors.CommandInputConverterNotInferrable,
                option.Property.Locations.FirstOrDefault(),
                option.Property.Name,
                option.Property.Type.GetGloballyQualifiedName()
            );

            return null;
        }

        // lang=csharp
        return $$"""
            new global::CliFx.Binding.CommandOptionDescriptor<{{command.Type.GetGloballyQualifiedName()}}, {{option.Property.Type.GetGloballyQualifiedName()}}>(
                new global::CliFx.Binding.PropertyDescriptor<{{command.Type.GetGloballyQualifiedName()}}, {{option.Property.Type.GetGloballyQualifiedName()}}>(
                    {{CSharp.Encode(option.Property.Name)}},
                    c => c.{{option.Property.Name}},
                    (c, v) => c.{{option.Property.Name}} = v
                ),
                {{CSharp.Encode(option.Name)}},
                {{CSharp.Encode(option.ShortName)}},
                {{CSharp.Encode(option.EnvironmentVariable)}},
                {{option.IsRequired.ToString().ToLowerInvariant()}},
                {{CSharp.Encode(option.Description)}},
                {{converterFragment}},
                {{EmitValidators(
                option.ValidatorTypes,
                option.Property.Type.GetGloballyQualifiedName()
            )}}
            )
            """;
    }

    private static string TryEmitCommandHelpOptionDescriptor(
        CommandSymbol command,
        DiagnosticReporter diagnostics
    )
    {
        // Check if the user already implemented a custom help option
        if (command.Type.Implements("CliFx.ICommandWithHelpOption"))
        {
            // Ensure that the user bound their IsHelpRequested property to an option
            var isHelpRequestedProperty = command
                .Type.GetProperties()
                .FirstOrDefault(p =>
                    string.Equals(p.Name, "IsHelpRequested", StringComparison.Ordinal)
                );

            var hasOptionBinding =
                isHelpRequestedProperty?.TryGetAttribute("CliFx.Binding.CommandOptionAttribute")
                is not null;

            if (!hasOptionBinding)
            {
                diagnostics.Report(
                    DiagnosticDescriptors.CommandHelpOptionPropertyMustBeBound,
                    isHelpRequestedProperty?.Locations.FirstOrDefault()
                        ?? command.Type.Locations.FirstOrDefault(),
                    "IsHelpRequested"
                );
            }

            return null;
        }

        // Check if the help option is being shadowed by an existing option
        var shadowingOptionByName = command.Options.FirstOrDefault(o =>
            string.Equals(o.Name, "help", StringComparison.OrdinalIgnoreCase)
        );

        var shadowingOptionByShortName = command.Options.FirstOrDefault(o => o.ShortName == 'h');

        var shadowingOption = shadowingOptionByShortName ?? shadowingOptionByName;

        if (shadowingOption is not null)
        {
            diagnostics.Report(
                DiagnosticDescriptors.CommandOptionShadowsConventionalHelpOption,
                shadowingOption.Property.Locations.FirstOrDefault(),
                shadowingOption.Property.Name,
                shadowingOptionByName is not null ? "name" : "short name"
            );

            return null;
        }

        // lang=csharp
        return $$"""
            new global::CliFx.Binding.CommandOptionDescriptor<{{command.Type.GetGloballyQualifiedName()}}, bool>(
                new global::CliFx.Binding.PropertyDescriptor<{{command.Type.GetGloballyQualifiedName()}}, bool>(
                    {{CSharp.Encode("IsHelpRequested")}},
                    c => c.IsHelpRequested,
                    (c, v) => c.IsHelpRequested = v
                ),
                {{CSharp.Encode("help")}},
                {{CSharp.Encode('h')}},
                null,
                false,
                {{CSharp.Encode("Shows help text.")}},
                new global::CliFx.Activation.BoolScalarInputConverter(),
                global::System.Array.Empty<global::CliFx.Activation.InputValidator<bool>>()
            )
            """;
    }

    private static string TryEmitCommandVersionOptionDescriptor(
        CommandSymbol command,
        DiagnosticReporter diagnostics
    )
    {
        // Check if the user already implemented a custom version option
        if (command.Type.Implements("CliFx.ICommandWithVersionOption"))
        {
            var isVersionRequestedProperty = command
                .Type.GetProperties()
                .FirstOrDefault(p =>
                    string.Equals(p.Name, "IsVersionRequested", StringComparison.Ordinal)
                );

            var hasOptionBinding =
                isVersionRequestedProperty?.TryGetAttribute("CliFx.Binding.CommandOptionAttribute")
                is not null;

            if (!hasOptionBinding)
            {
                diagnostics.Report(
                    DiagnosticDescriptors.CommandVersionOptionPropertyMustBeBound,
                    isVersionRequestedProperty?.Locations.FirstOrDefault()
                        ?? command.Type.Locations.FirstOrDefault(),
                    "IsVersionRequested"
                );
            }

            return null;
        }

        // Only default commands get the conventional version option
        if (!command.IsDefault)
            return null;

        // Check if the version option is being shadowed by an existing option
        var shadowingOptionByName = command.Options.FirstOrDefault(o =>
            string.Equals(o.Name, "version", StringComparison.OrdinalIgnoreCase)
        );

        if (shadowingOptionByName is not null)
        {
            diagnostics.Report(
                DiagnosticDescriptors.CommandOptionShadowsConventionalVersionOption,
                shadowingOptionByName.Property.Locations.FirstOrDefault(),
                shadowingOptionByName.Property.Name,
                "name"
            );

            return null;
        }

        // lang=csharp
        return $$"""
            new global::CliFx.Binding.CommandOptionDescriptor<{{command.Type.GetGloballyQualifiedName()}}, bool>(
                new global::CliFx.Binding.PropertyDescriptor<{{command.Type.GetGloballyQualifiedName()}}, bool>(
                    {{CSharp.Encode("IsVersionRequested")}},
                    c => c.IsVersionRequested,
                    (c, v) => c.IsVersionRequested = v
                ),
                {{CSharp.Encode("version")}},
                null,
                null,
                false,
                {{CSharp.Encode("Shows version information.")}},
                new global::CliFx.Activation.BoolScalarInputConverter(),
                global::System.Array.Empty<global::CliFx.Activation.InputValidator<bool>>()
            )
            """;
    }

    private static string? TryEmitCommandInputDescriptor(
        CommandSymbol command,
        CommandInputSymbol input,
        DiagnosticReporter diagnostics
    ) =>
        input switch
        {
            CommandParameterSymbol parameter => TryEmitCommandParameterDescriptor(
                command,
                parameter,
                diagnostics
            ),
            CommandOptionSymbol option => TryEmitCommandOptionDescriptor(
                command,
                option,
                diagnostics
            ),
            _ => throw new InvalidOperationException(
                $"Unknown input type '{input.GetType().Name}'."
            ),
        };

    internal static string Emit(CommandSymbol command, DiagnosticReporter diagnostics) =>
        // lang=csharp
        $$"""
        // <auto-generated />

        {{(command.Type.TryGetNamespaceName() is { } namespaceName
            ? $"namespace {namespaceName};"
            : ""
        )}}

        {{string.Concat(command.Type.GetContainingTypes().Reverse().Select(t =>
            // lang=csharp
            $$"""
            partial class {{t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}}
            {
            """
        ))}}

        partial class {{command.Type.Name}}
        {
            /// <summary>
            /// Generated command descriptor for <see cref="{{Xml.Escape(command.Type.GetGloballyQualifiedName())}}" /> ({{Xml.Escape(command.ToString())}}).
            /// </summary>
            /// <remarks>
            /// <list type="bullet">
                {{string.Join(
                    Environment.NewLine,
                    command.Inputs.Select(i =>
                        // lang=xml
                        $"""
                        /// <item>
                        /// <see cref="{Xml.Escape(i.Property.GetGloballyQualifiedName())}" /> ({Xml.Escape(i.ToString())})
                        /// </item>
                        """
                    )
                ).NullIfWhiteSpace() ?? "///"}}
            /// </list>
            /// </remarks>
            public static global::CliFx.Binding.CommandDescriptor Descriptor { get; } =
                new global::CliFx.Binding.CommandDescriptor<{{command.Type.GetGloballyQualifiedName()}}>(
                    {{CSharp.Encode(command.Name)}},
                    {{CSharp.Encode(command.Description)}},
                    new global::CliFx.Binding.CommandInputDescriptor[]
                    {
                        {{string.Join(",", command.Inputs
                            .Select(i => TryEmitCommandInputDescriptor(
                                command,
                                i,
                                diagnostics
                            ))
                            .Append(
                                TryEmitCommandHelpOptionDescriptor(command, diagnostics) is {} helpOptionDescriptorFragment
                                    ? helpOptionDescriptorFragment
                                    : null
                            )
                            .Append(
                                TryEmitCommandVersionOptionDescriptor(command, diagnostics) is {} versionOptionDescriptorFragment
                                    ? versionOptionDescriptorFragment
                                    : null
                            )
                            .WhereNotNull()
                        )}}
                    }
                );
        }

        {{(!command.Type.Implements("CliFx.ICommandWithHelpOption")
            ? // lang=csharp
                $$"""
                partial class {{command.Type.Name}} : global::CliFx.ICommandWithHelpOption
                {
                    /// <inheritdoc />
                    public bool IsHelpRequested { get; set; }
                }
                """
            : ""
        )}}

        {{(command.IsDefault && !command.Type.Implements("CliFx.ICommandWithVersionOption")
            ? // lang=csharp
                $$"""
                partial class {{command.Type.Name}} : global::CliFx.ICommandWithVersionOption
                {
                    /// <inheritdoc />
                    public bool IsVersionRequested { get; set; }
                }
                """
            : ""
        )}}

        {{string.Concat(command.Type.GetContainingTypes().Reverse().Select(_ => "}"))}}
        """;
}
