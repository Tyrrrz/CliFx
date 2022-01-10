using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ParameterMustBeLastIfNonRequiredAnalyzer : AnalyzerBase
{
    public ParameterMustBeLastIfNonRequiredAnalyzer()
        : base(
            "Parameters marked as non-required must be the last in order",
            "This parameter is non-required so it must be the last in order (its order must be highest within the command).")
    {
    }

    private void Analyze(
        SyntaxNodeAnalysisContext context,
        PropertyDeclarationSyntax propertyDeclaration,
        IPropertySymbol property)
    {
        if (property.ContainingType is null)
            return;

        var parameter = CommandParameterSymbol.TryResolve(property);
        if (parameter is null)
            return;

        if (parameter.IsRequired != false)
            return;

        var otherProperties = property
            .ContainingType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(m => !m.Equals(property, SymbolEqualityComparer.Default))
            .ToArray();

        foreach (var otherProperty in otherProperties)
        {
            var otherParameter = CommandParameterSymbol.TryResolve(otherProperty);
            if (otherParameter is null)
                continue;

            if (otherParameter.Order > parameter.Order)
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