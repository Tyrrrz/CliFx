using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CommandSchemaAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            DiagnosticDescriptors.CliFx0001,
            DiagnosticDescriptors.CliFx0002,
            DiagnosticDescriptors.CliFx0021,
            DiagnosticDescriptors.CliFx0022,
            DiagnosticDescriptors.CliFx0023,
            DiagnosticDescriptors.CliFx0024,
            DiagnosticDescriptors.CliFx0025,
            DiagnosticDescriptors.CliFx0026,
            DiagnosticDescriptors.CliFx0041,
            DiagnosticDescriptors.CliFx0042,
            DiagnosticDescriptors.CliFx0043,
            DiagnosticDescriptors.CliFx0044,
            DiagnosticDescriptors.CliFx0045,
            DiagnosticDescriptors.CliFx0046,
            DiagnosticDescriptors.CliFx0047
        );

        private static bool IsScalarType(ITypeSymbol typeSymbol) =>
            KnownSymbols.IsSystemString(typeSymbol) ||
            !typeSymbol.AllInterfaces.Select(i => i.ConstructedFrom)
                .Any(KnownSymbols.IsSystemCollectionsGenericIEnumerable);

        private static void CheckCommandParameterProperties(
            SymbolAnalysisContext context,
            IReadOnlyList<IPropertySymbol> properties)
        {
            var parameters = properties
                .Select(p =>
                {
                    var attribute = p
                        .GetAttributes()
                        .First(a => KnownSymbols.IsCommandParameterAttribute(a.AttributeClass));

                    var order = attribute
                        .ConstructorArguments
                        .Select(a => a.Value)
                        .FirstOrDefault() as int?;

                    var name = attribute
                        .NamedArguments
                        .Where(a => a.Key == "Name")
                        .Select(a => a.Value.Value)
                        .FirstOrDefault() as string;

                    var converter = attribute
                        .NamedArguments
                        .Where(a => a.Key == "Converter")
                        .Select(a => a.Value.Value)
                        .Cast<ITypeSymbol?>()
                        .FirstOrDefault();

                    var validators = attribute
                        .NamedArguments
                        .Where(a => a.Key == "Validators")
                        .SelectMany(a => a.Value.Values)
                        .Select(c => c.Value)
                        .Cast<ITypeSymbol>()
                        .ToArray();

                    return new
                    {
                        Property = p,
                        Order = order,
                        Name = name,
                        Converter = converter,
                        Validators = validators
                    };
                })
                .ToArray();

            // Duplicate order
            var duplicateOrderParameters = parameters
                .Where(p => p.Order != null)
                .GroupBy(p => p.Order)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.AsEnumerable())
                .ToArray();

            foreach (var parameter in duplicateOrderParameters)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.CliFx0021, parameter.Property.Locations.First()
                ));
            }

            // Duplicate name
            var duplicateNameParameters = parameters
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.AsEnumerable())
                .ToArray();

            foreach (var parameter in duplicateNameParameters)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.CliFx0022, parameter.Property.Locations.First()
                ));
            }

            // Multiple non-scalar
            var nonScalarParameters = parameters
                .Where(p => !IsScalarType(p.Property.Type))
                .ToArray();

            if (nonScalarParameters.Length > 1)
            {
                foreach (var parameter in nonScalarParameters)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.CliFx0023, parameter.Property.Locations.First()
                    ));
                }
            }

            // Non-last non-scalar
            var nonLastNonScalarParameter = parameters
                .OrderByDescending(a => a.Order)
                .Skip(1)
                .LastOrDefault(p => !IsScalarType(p.Property.Type));

            if (nonLastNonScalarParameter != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.CliFx0024, nonLastNonScalarParameter.Property.Locations.First()
                ));
            }

            // Invalid converter
            var invalidConverterParameters = parameters
                .Where(p =>
                    p.Converter != null &&
                    !p.Converter.AllInterfaces.Any(KnownSymbols.IsArgumentValueConverterInterface))
                .ToArray();

            foreach (var parameter in invalidConverterParameters)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.CliFx0025, parameter.Property.Locations.First()
                ));
            }

            // Invalid validators
            var invalidValidatorsParameters = parameters
                .Where(p => !p.Validators.All(v => v.AllInterfaces.Any(KnownSymbols.IsArgumentValueValidatorInterface)))
                .ToArray();

            foreach (var parameter in invalidValidatorsParameters)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.CliFx0026, parameter.Property.Locations.First()
                ));
            }
        }

        private static void CheckCommandOptionProperties(
            SymbolAnalysisContext context,
            IReadOnlyList<IPropertySymbol> properties)
        {
            var options = properties
                .Select(p =>
                {
                    var attribute = p
                        .GetAttributes()
                        .First(a => KnownSymbols.IsCommandOptionAttribute(a.AttributeClass));

                    var name = attribute
                        .ConstructorArguments
                        .Where(a => KnownSymbols.IsSystemString(a.Type))
                        .Select(a => a.Value)
                        .FirstOrDefault() as string;

                    var shortName = attribute
                        .ConstructorArguments
                        .Where(a => KnownSymbols.IsSystemChar(a.Type))
                        .Select(a => a.Value)
                        .FirstOrDefault() as char?;

                    var envVarName = attribute
                        .NamedArguments
                        .Where(a => a.Key == "EnvironmentVariableName")
                        .Select(a => a.Value.Value)
                        .FirstOrDefault() as string;

                    var converter = attribute
                        .NamedArguments
                        .Where(a => a.Key == "Converter")
                        .Select(a => a.Value.Value)
                        .Cast<ITypeSymbol>()
                        .FirstOrDefault();

                    var validators = attribute
                        .NamedArguments
                        .Where(a => a.Key == "Validators")
                        .SelectMany(a => a.Value.Values)
                        .Select(c => c.Value)
                        .Cast<ITypeSymbol>()
                        .ToArray();

                    return new
                    {
                        Property = p,
                        Name = name,
                        ShortName = shortName,
                        EnvironmentVariableName = envVarName,
                        Converter = converter,
                        Validators = validators
                    };
                })
                .ToArray();

            // No name
            var noNameOptions = options
                .Where(o => string.IsNullOrWhiteSpace(o.Name) && o.ShortName == null)
                .ToArray();

            foreach (var option in noNameOptions)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.CliFx0041, option.Property.Locations.First()
                ));
            }

            // Too short name
            var invalidNameLengthOptions = options
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Length <= 1)
                .ToArray();

            foreach (var option in invalidNameLengthOptions)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.CliFx0042, option.Property.Locations.First()
                ));
            }

            // Duplicate name
            var duplicateNameOptions = options
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.AsEnumerable())
                .ToArray();

            foreach (var option in duplicateNameOptions)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.CliFx0043, option.Property.Locations.First()
                ));
            }

            // Duplicate name
            var duplicateShortNameOptions = options
                .Where(p => p.ShortName != null)
                .GroupBy(p => p.ShortName)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.AsEnumerable())
                .ToArray();

            foreach (var option in duplicateShortNameOptions)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.CliFx0044, option.Property.Locations.First()
                ));
            }

            // Duplicate environment variable name
            var duplicateEnvironmentVariableNameOptions = options
                .Where(p => !string.IsNullOrWhiteSpace(p.EnvironmentVariableName))
                .GroupBy(p => p.EnvironmentVariableName, StringComparer.Ordinal)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.AsEnumerable())
                .ToArray();

            foreach (var option in duplicateEnvironmentVariableNameOptions)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.CliFx0045, option.Property.Locations.First()
                ));
            }

            // Invalid converter
            var invalidConverterOptions = options
                .Where(o =>
                    o.Converter != null &&
                    !o.Converter.AllInterfaces.Any(KnownSymbols.IsArgumentValueConverterInterface))
                .ToArray();

            foreach (var option in invalidConverterOptions)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.CliFx0046, option.Property.Locations.First()
                ));
            }

            // Invalid validators
            var invalidValidatorsOptions = options
                .Where(p => !p.Validators.All(v => v.AllInterfaces.Any(KnownSymbols.IsArgumentValueValidatorInterface)))
                .ToArray();

            foreach (var option in invalidValidatorsOptions)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.CliFx0047, option.Property.Locations.First()
                ));
            }
        }

        private static void CheckCommandType(SymbolAnalysisContext context)
        {
            // Named type: MyCommand
            if (!(context.Symbol is INamedTypeSymbol namedTypeSymbol) ||
                namedTypeSymbol.TypeKind != TypeKind.Class)
                return;

            // Implements ICommand?
            var implementsCommandInterface = namedTypeSymbol
                .AllInterfaces
                .Any(KnownSymbols.IsCommandInterface);

            // Has CommandAttribute?
            var hasCommandAttribute = namedTypeSymbol
                .GetAttributes()
                .Select(a => a.AttributeClass)
                .Any(KnownSymbols.IsCommandAttribute);

            var isValidCommandType =
                // implements interface
                implementsCommandInterface && (
                    // and either abstract class or has attribute
                    namedTypeSymbol.IsAbstract || hasCommandAttribute
                );

            if (!isValidCommandType)
            {
                // See if this was meant to be a command type (either interface or attribute present)
                var isAlmostValidCommandType = implementsCommandInterface ^ hasCommandAttribute;

                if (isAlmostValidCommandType && !implementsCommandInterface)
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CliFx0001,
                        namedTypeSymbol.Locations.First()));

                if (isAlmostValidCommandType && !hasCommandAttribute)
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CliFx0002,
                        namedTypeSymbol.Locations.First()));

                return;
            }

            var properties = namedTypeSymbol
                .GetMembers()
                .Where(m => m.Kind == SymbolKind.Property)
                .OfType<IPropertySymbol>().ToArray();

            // Check parameters
            var parameterProperties = properties
                .Where(p => p
                    .GetAttributes()
                    .Select(a => a.AttributeClass)
                    .Any(KnownSymbols.IsCommandParameterAttribute))
                .ToArray();

            CheckCommandParameterProperties(context, parameterProperties);

            // Check options
            var optionsProperties = properties
                .Where(p => p
                    .GetAttributes()
                    .Select(a => a.AttributeClass)
                    .Any(KnownSymbols.IsCommandOptionAttribute))
                .ToArray();

            CheckCommandOptionProperties(context, optionsProperties);
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(CheckCommandType, SymbolKind.NamedType);
        }
    }
}