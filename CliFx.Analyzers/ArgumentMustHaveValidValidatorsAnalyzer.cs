using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    public class ArgumentMustHaveValidValidatorsAnalyzer : AnalyzerBase
    {
        public ArgumentMustHaveValidValidatorsAnalyzer()
            : base(
                $"Parameter and option validators must derive from `{SymbolNames.CliFxArgumentValidatorClass}`",
                $"All validators specified for this parameter or option must derive from `{SymbolNames.CliFxArgumentValidatorClass}`.")
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
                (ICommandMemberSymbol)
                CommandParameterSymbol.TryResolve(property) ??
                CommandOptionSymbol.TryResolve(property);

            if (argument is null)
                return;

            foreach (var validatorType in argument.ValidatorTypes)
            {
                // We check against an internal interface because checking against a generic class is a pain
                var validatorImplementsInterface = validatorType
                    .AllInterfaces
                    .Any(s => s.DisplayNameMatches(SymbolNames.CliFxArgumentValidatorInterface));

                if (!validatorImplementsInterface)
                {
                    context.ReportDiagnostic(CreateDiagnostic(propertyDeclaration.GetLocation()));

                    // No need to report multiple identical diagnostics on the same node
                    break;
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