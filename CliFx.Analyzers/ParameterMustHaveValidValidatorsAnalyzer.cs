using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ParameterMustHaveValidValidatorsAnalyzer : AnalyzerBase
    {
        public ParameterMustHaveValidValidatorsAnalyzer()
            : base(
                $"Parameter validators must derive from `{SymbolNames.CliFxBindingValidatorClass}`",
                $"All validators specified for this parameter must derive from `{SymbolNames.CliFxBindingValidatorClass}`.")
        {
        }

        private void Analyze(
            SyntaxNodeAnalysisContext context,
            PropertyDeclarationSyntax propertyDeclaration,
            IPropertySymbol property)
        {
            var parameter = CommandParameterSymbol.TryResolve(property);
            if (parameter is null)
                return;

            foreach (var validatorType in parameter.ValidatorTypes)
            {
                // We check against an internal interface because checking against a generic class is a pain
                var validatorImplementsInterface = validatorType
                    .AllInterfaces
                    .Any(s => s.DisplayNameMatches(SymbolNames.CliFxBindingValidatorInterface));

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
            context.HandlePropertyDeclaration(Analyze);
        }
    }
}