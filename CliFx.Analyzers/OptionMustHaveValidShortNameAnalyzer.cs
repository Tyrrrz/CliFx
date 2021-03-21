using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OptionMustHaveValidShortNameAnalyzer : AnalyzerBase
    {
        public OptionMustHaveValidShortNameAnalyzer()
            : base(
                "Option short names must be letter characters",
                "This option's short name must be a single letter character.")
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

            if (option.ShortName is null)
                return;

            if (!char.IsLetter(option.ShortName.Value))
            {
                context.ReportDiagnostic(CreateDiagnostic(propertyDeclaration.GetLocation()));
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);
            context.HandlePropertyDeclaration(Analyze);
        }
    }
}