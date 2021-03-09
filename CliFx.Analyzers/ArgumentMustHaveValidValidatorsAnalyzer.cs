using System.Linq;
using CliFx.Analyzers.ObjectModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    public class ArgumentMustHaveValidValidatorsAnalyzer : AnalyzerBase
    {
        public ArgumentMustHaveValidValidatorsAnalyzer()
            : base(
                $"Parameter and option validators must derive from `{KnownSymbols.CliFxArgumentValueValidatorClass}`",
                $"All validators specified for this parameter or option must derive from `{KnownSymbols.CliFxArgumentValueValidatorClass}`.")
        {
        }

        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
                return;

            var property = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
            if (property is null)
                return;

            var argument =
                (ICommandArgumentSymbol)
                CommandParameterSymbol.TryResolve(property) ??
                CommandOptionSymbol.TryResolve(property);

            if (argument is null)
                return;

            foreach (var validatorType in argument.ValidatorTypes)
            {
                if (!validatorType.AllInterfaces.Any(KnownSymbols.IsArgumentValueValidatorInterface))
                {
                    context.ReportDiagnostic(CreateDiagnostic(propertyDeclaration.GetLocation()));
                }
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.PropertyDeclaration);
        }
    }
}