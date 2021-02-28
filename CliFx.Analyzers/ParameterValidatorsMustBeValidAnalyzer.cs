using System.Collections.Immutable;
using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ParameterValidatorsMustBeValidAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor DiagnosticDescriptor { get; } = new(
            "CliFx_" + nameof(ParameterValidatorsMustBeValidAnalyzer).TrimEnd("Analyzer"),
            "Parameter validator type must derive from `CliFx.ArgumentValueValidator<T>`",
            "Type must implement `CliFx.CliFx.ArgumentValueValidator<T>` in order to be used as a validator.",
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

            foreach (var validatorType in parameter.ValidatorTypes)
            {
                if (!validatorType.AllInterfaces.Any(KnownSymbols.IsArgumentValueValidatorInterface))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptor,
                        propertyDeclaration.GetLocation()
                    ));
                }
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