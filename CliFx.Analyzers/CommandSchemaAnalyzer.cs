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
            DiagnosticDescriptors.CliFx0041,
            DiagnosticDescriptors.CliFx0042,
            DiagnosticDescriptors.CliFx0043,
            DiagnosticDescriptors.CliFx0044,
            DiagnosticDescriptors.CliFx0045
        );

        private static bool IsScalarType(ITypeSymbol typeSymbol) =>
            KnownSymbols.IsSystemString(typeSymbol) ||
            !typeSymbol.AllInterfaces.Select(i => i.ConstructedFrom).Any(KnownSymbols.IsSystemCollectionsGenericIEnumerable);

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

                    return new
                    {
                        Property = p,
                        Order = order,
                        Name = name
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
                context.ReportDiagnostic(
                    Diagnostic.Create(DiagnosticDescriptors.CliFx0021, parameter.Property.Locations.First()));
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
                context.ReportDiagnostic(
                    Diagnostic.Create(DiagnosticDescriptors.CliFx0022, parameter.Property.Locations.First()));
            }

            // Multiple non-scalar
            var nonScalarParameters = parameters
                .Where(p => !IsScalarType(p.Property.Type))
                .ToArray();

            if (nonScalarParameters.Length > 1)
            {
                foreach (var parameter in nonScalarParameters)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors.CliFx0023, parameter.Property.Locations.First()));
                }
            }

            // Non-last non-scalar
            var nonLastNonScalarParameter = parameters
                .OrderByDescending(a => a.Order)
                .Skip(1)
                .LastOrDefault(p => !IsScalarType(p.Property.Type));

            if (nonLastNonScalarParameter != null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(DiagnosticDescriptors.CliFx0024, nonLastNonScalarParameter.Property.Locations.First()));
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

                    return new
                    {
                        Property = p,
                        Name = name,
                        ShortName = shortName
                    };
                })
                .ToArray();

            // No name
            var noNameOptions = options
                .Where(o => string.IsNullOrWhiteSpace(o.Name) && o.ShortName == null)
                .ToArray();

            foreach (var option in noNameOptions)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(DiagnosticDescriptors.CliFx0041, option.Property.Locations.First()));
            }

            // Too short name
            var invalidNameLengthOptions = options
                .Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Length <= 1)
                .ToArray();

            foreach (var option in invalidNameLengthOptions)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(DiagnosticDescriptors.CliFx0042, option.Property.Locations.First()));
            }
        }

        private static void CheckCommandType(SymbolAnalysisContext context)
        {
            // Named type: MyCommand
            if (!(context.Symbol is INamedTypeSymbol namedTypeSymbol))
                return;

            // Only instantiable classes
            if (namedTypeSymbol.TypeKind != TypeKind.Class || namedTypeSymbol.IsAbstract)
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
                // implement interface
                implementsCommandInterface && (
                    // and either abstract class or interface
                    namedTypeSymbol.TypeKind == TypeKind.Interface || namedTypeSymbol.IsAbstract ||
                    // or has attribute
                    hasCommandAttribute
                );

            if (!isValidCommandType)
            {
                // See if this was meant to be a command type (either interface or attribute present)
                var isAlmostValidCommandType = implementsCommandInterface ^ hasCommandAttribute;

                if (isAlmostValidCommandType && !implementsCommandInterface)
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CliFx0001, namedTypeSymbol.Locations.First()));

                if (isAlmostValidCommandType && !hasCommandAttribute)
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CliFx0002, namedTypeSymbol.Locations.First()));

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

            CheckCommandParameterProperties(context, parameterProperties);
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