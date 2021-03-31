using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OptionMustHaveUniqueShortNameAnalyzer : AnalyzerBase
    {
        public OptionMustHaveUniqueShortNameAnalyzer()
            : base(
                "Options must have unique short names",
                "This option's short name must be unique within the command (comparison IS case sensitive).")
        {
        }

        private void Analyze(
            SyntaxNodeAnalysisContext context,
            PropertyDeclarationSyntax propertyDeclaration,
            IPropertySymbol property)
        {
            if (property.ContainingType is null)
                return;

            var option = CommandOptionSymbol.TryResolve(property);
            if (option is null)
                return;

            if (option.ShortName is null)
                return;

            var otherProperties = property
                .ContainingType
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(m => !m.Equals(property, SymbolEqualityComparer.Default))
                .ToArray();

            foreach (var otherProperty in otherProperties)
            {
                var otherOption = CommandOptionSymbol.TryResolve(otherProperty);
                if (otherOption is null)
                    continue;

                if (otherOption.ShortName is null)
                    continue;

                if (option.ShortName == otherOption.ShortName)
                {
                    context.ReportDiagnostic(CreateDiagnostic(propertyDeclaration.GetLocation()));
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