using System;
using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ParameterMustBeLastIfNotRequiredAnalyzer : AnalyzerBase
{
    public ParameterMustBeLastIfNotRequiredAnalyzer()
        : base(
            "Only last parameter can be optional",
            "IsRequired can only be set to false on the last parameter.")
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
        
        if (parameter is null || parameter.IsRequired is null or true )
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

            if (parameter.IsRequired is false && parameter.Order < otherParameter.Order)
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