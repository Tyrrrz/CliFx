using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Generators.Utils.Extensions;
using Microsoft.CodeAnalysis;

namespace CliFx.Generators.SemanticModel;

internal sealed class CommandDescriptorBuilder(CliFxReferences refs)
{
    // Used to check converter inferrability during descriptor building.
    private readonly CommandSchemaEmitter _emitter = new(refs);

    /// <summary>
    /// Attempts to build a <see cref="CommandDescriptor"/> from <paramref name="type"/>.
    /// Returns <see langword="null"/> if the type does not satisfy the requirements for
    /// schema generation (not annotated with <c>[Command]</c>, abstract, etc.).
    /// </summary>
    internal CommandDescriptor? TryBuild(INamedTypeSymbol type)
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
                var descriptor = BuildParameter(property, parameterAttribute, diagnostics);
                if (descriptor is not null)
                    parameterDescriptors.Add(descriptor);
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
                var descriptor = BuildOption(property, optionAttribute, diagnostics);
                if (descriptor is not null)
                    optionDescriptors.Add(descriptor);
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

    private CommandParameterDescriptor? BuildParameter(
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

        var converterType = (
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "Converter").Value.Value
            as ITypeSymbol
        )
            is { } converterSym
            ? new TypeDescriptor(converterSym)
            : null;

        // Emit an error and drop the property if no converter can be inferred.
        if (_emitter.TryBuildConverterExpr(converterType, property.Symbol) is null)
        {
            diagnostics.Add(
                Diagnostic.Create(
                    DiagnosticDescriptors.ConverterNotInferrable,
                    property.Symbol.Locations.FirstOrDefault() ?? Location.None,
                    property.Name,
                    property.Type.FullyQualifiedName
                )
            );
            return null;
        }

        return new CommandParameterDescriptor(
            property,
            order,
            name,
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "Description").Value.Value
                as string,
            converterType,
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

    private CommandOptionDescriptor? BuildOption(
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

        var converterType = (
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "Converter").Value.Value
            as ITypeSymbol
        )
            is { } converterSym
            ? new TypeDescriptor(converterSym)
            : null;

        // Emit an error and drop the property if no converter can be inferred.
        if (_emitter.TryBuildConverterExpr(converterType, property.Symbol) is null)
        {
            diagnostics.Add(
                Diagnostic.Create(
                    DiagnosticDescriptors.ConverterNotInferrable,
                    property.Symbol.Locations.FirstOrDefault() ?? Location.None,
                    property.Name,
                    property.Type.FullyQualifiedName
                )
            );
            return null;
        }

        return new CommandOptionDescriptor(
            property,
            name,
            shortName,
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "EnvironmentVariable").Value.Value
                as string,
            attribute.NamedArguments.FirstOrDefault(a => a.Key == "Description").Value.Value
                as string,
            converterType,
            attribute
                .NamedArguments.Where(a => a.Key == "Validators")
                .SelectMany(a => a.Value.Values)
                .Select(v => v.Value)
                .OfType<ITypeSymbol>()
                .Select(s => new TypeDescriptor(s))
                .ToArray()
        );
    }
}
