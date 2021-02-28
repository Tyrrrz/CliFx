using System.Collections.Immutable;
using System.Linq;
using CliFx.Analyzers.ObjectModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ParameterConverterMustBeValidAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor DiagnosticDescriptor { get; } = new(
            "CliFx_ParameterConverterMustBeValid",
            "Parameter converter type must implement `IArgumentValueConverter`",
            "Type must implement `CliFx.IArgumentValueConverter` in order to be used as converter.",
            "CliFx", DiagnosticSeverity.Error, true
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(DiagnosticDescriptor);

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
                return;

            var property = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
            if (property is null)
                return;

            var parameter = CommandParameterSymbol.TryResolve(property);
            if (parameter is null)
                return;

            if (parameter.ConverterType is null)
                return;

            if (!parameter.ConverterType.AllInterfaces.Any(KnownSymbols.IsArgumentValueConverterInterface))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptor,
                    propertyDeclaration.GetLocation()
                ));
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.PropertyDeclaration);
        }
    }
}