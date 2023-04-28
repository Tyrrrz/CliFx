using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ParameterMustBeRequiredIfPropertyRequiredAnalyzer : AnalyzerBase
{
    public ParameterMustBeRequiredIfPropertyRequiredAnalyzer()
        : base(
            "Parameters bound to required properties cannot be marked as non-required",
            "This parameter cannot be marked as non-required because it's bound to a required property.")
    {
    }

    private void Analyze(
        SyntaxNodeAnalysisContext context,
        PropertyDeclarationSyntax propertyDeclaration,
        IPropertySymbol property)
    {
        if (property.ContainingType is null)
            return;

        if (!property.IsRequired())
            return;

        var parameter = CommandParameterSymbol.TryResolve(property);
        if (parameter is null)
            return;

        if (parameter.IsRequired != false)
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