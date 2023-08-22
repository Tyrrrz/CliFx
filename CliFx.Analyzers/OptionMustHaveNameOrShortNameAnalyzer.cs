using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OptionMustHaveNameOrShortNameAnalyzer : AnalyzerBase
{
    public OptionMustHaveNameOrShortNameAnalyzer()
        : base(
            "Options must have either a name or short name specified",
            "This option must have either a name or short name specified."
        ) { }

    private void Analyze(
        SyntaxNodeAnalysisContext context,
        PropertyDeclarationSyntax propertyDeclaration,
        IPropertySymbol property
    )
    {
        var option = CommandOptionSymbol.TryResolve(property);
        if (option is null)
            return;

        if (string.IsNullOrWhiteSpace(option.Name) && option.ShortName is null)
        {
            context.ReportDiagnostic(
                CreateDiagnostic(propertyDeclaration.Identifier.GetLocation())
            );
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);
        context.HandlePropertyDeclaration(Analyze);
    }
}
