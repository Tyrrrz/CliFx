using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OptionMustHaveValidValidatorsAnalyzer : AnalyzerBase
    {
        public OptionMustHaveValidValidatorsAnalyzer()
            : base(
                $"Option validators must derive from `{SymbolNames.CliFxBindingValidatorClass}`",
                $"All validators specified for this option must derive from `{SymbolNames.CliFxBindingValidatorClass}`.")
        {
        }

        private void Analyze(
            SyntaxNodeAnalysisContext context,
            PropertyDeclarationSyntax propertyDeclaration,
            IPropertySymbol property)
        {
            var option = CommandOptionSymbol.TryResolve(property);
            if (option is null)
                return;

            foreach (var validatorType in option.ValidatorTypes)
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