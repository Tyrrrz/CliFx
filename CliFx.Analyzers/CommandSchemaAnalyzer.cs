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
            DiagnosticDescriptors.CliFx0003,
            DiagnosticDescriptors.CliFx0002,
            DiagnosticDescriptors.CliFx0004,
            DiagnosticDescriptors.CliFx0005
        );

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
                    Diagnostic.Create(DiagnosticDescriptors.CliFx0004, parameter.Property.Locations.First()));
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
                    Diagnostic.Create(DiagnosticDescriptors.CliFx0005, parameter.Property.Locations.First()));
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
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CliFx0002, namedTypeSymbol.Locations.First()));

                if (isAlmostValidCommandType && !hasCommandAttribute)
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CliFx0003, namedTypeSymbol.Locations.First()));

                return;
            }

            // Check parameters
            var parameterProperties = namedTypeSymbol
                .GetMembers()
                .Where(m => m.Kind == SymbolKind.Property)
                .OfType<IPropertySymbol>()
                .Where(p => p
                    .GetAttributes()
                    .Select(a => a.AttributeClass)
                    .Any(KnownSymbols.IsCommandParameterAttribute))
                .ToArray();

            CheckCommandParameterProperties(context, parameterProperties);
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(CheckCommandType, SymbolKind.NamedType);
        }
    }
}