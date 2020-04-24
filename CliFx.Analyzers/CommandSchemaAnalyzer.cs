using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CommandSchemaAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor.CliFx0002, Descriptor.CliFx0003);

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

            if (!implementsCommandInterface ^ !hasCommandAttribute)
            {
                context.ReportDiagnostic(!implementsCommandInterface
                    ? Diagnostic.Create(Descriptor.CliFx0003, namedTypeSymbol.Locations.First())
                    : Diagnostic.Create(Descriptor.CliFx0002, namedTypeSymbol.Locations.First()));
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(CheckCommandType, SymbolKind.NamedType);
        }
    }
}