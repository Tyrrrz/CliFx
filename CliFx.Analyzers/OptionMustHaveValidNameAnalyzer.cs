using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OptionMustHaveValidNameAnalyzer : AnalyzerBase
{
    public OptionMustHaveValidNameAnalyzer()
        : base(
            "Options must have valid names",
            "This option's name must be at least 2 characters long and must start with a letter. " +
            "Specified name: `{0}`.")
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

        if (string.IsNullOrWhiteSpace(option.Name))
            return;

        if (option.Name.Length < 2 || !char.IsLetter(option.Name[0]))
        {
            context.ReportDiagnostic(
                CreateDiagnostic(
                    propertyDeclaration.Identifier.GetLocation(),
                    option.Name
                )
            );
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);
        context.HandlePropertyDeclaration(Analyze);
    }
}