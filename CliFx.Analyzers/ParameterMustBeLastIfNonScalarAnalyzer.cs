using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ParameterMustBeLastIfNonScalarAnalyzer : AnalyzerBase
{
    public ParameterMustBeLastIfNonScalarAnalyzer()
        : base(
            "Parameters of non-scalar types must be the last in order",
            "This parameter has a non-scalar type so it must be the last in order (its order must be highest within the command). " +
            "Property bound to another non-scalar parameter: `{0}`.")
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

        if (parameter.IsScalar())
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
                context.ReportDiagnostic(
                    CreateDiagnostic(
                        propertyDeclaration.Identifier.GetLocation(),
                        otherProperty.Name
                    )
                );
            }
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);
        context.HandlePropertyDeclaration(Analyze);
    }
}