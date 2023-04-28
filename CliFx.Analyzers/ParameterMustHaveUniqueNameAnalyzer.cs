using System;
using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ParameterMustHaveUniqueNameAnalyzer : AnalyzerBase
{
    public ParameterMustHaveUniqueNameAnalyzer()
        : base(
            "Parameters must have unique names",
            "This parameter's name must be unique within the command (comparison IS NOT case sensitive). " +
            "Specified name: `{0}`. " +
            "Property bound to another parameter with the same name: `{1}`.")
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

        if (string.IsNullOrWhiteSpace(parameter.Name))
            return;

        var otherProperties = property
            .ContainingType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(m => !m.Equals(property))
            .ToArray();

        foreach (var otherProperty in otherProperties)
        {
            var otherParameter = CommandParameterSymbol.TryResolve(otherProperty);
            if (otherParameter is null)
                continue;

            if (string.IsNullOrWhiteSpace(otherParameter.Name))
                continue;

            if (string.Equals(parameter.Name, otherParameter.Name, StringComparison.OrdinalIgnoreCase))
            {
                context.ReportDiagnostic(
                    CreateDiagnostic(
                        propertyDeclaration.Identifier.GetLocation(),
                        parameter.Name,
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