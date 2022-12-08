using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OptionMustBeRequiredIfPropertyRequiredAnalyzer : AnalyzerBase
{
    public OptionMustBeRequiredIfPropertyRequiredAnalyzer()
        : base(
            "Options bound to required properties cannot be marked as non-required",
            "This option cannot be marked as non-required because it's bound to a required property.")
    {
    }

    private void Analyze(
        SyntaxNodeAnalysisContext context,
        PropertyDeclarationSyntax propertyDeclaration,
        IPropertySymbol property)
    {
        if (property.ContainingType is null)
            return;

        if (!property.IsRequired)
            return;

        var option = CommandOptionSymbol.TryResolve(property);
        if (option is null)
            return;

        if (option.IsRequired != false)
            return;

        context.ReportDiagnostic(
            CreateDiagnostic(
                propertyDeclaration.Identifier.GetLocation()
            )
        );
    }

    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);
        context.HandlePropertyDeclaration(Analyze);
    }
}