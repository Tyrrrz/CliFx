using System;
using System.Linq;
using CliFx.Analyzers.ObjectModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    public class ParameterMustHaveUniqueNameAnalyzer : AnalyzerBase
    {
        public ParameterMustHaveUniqueNameAnalyzer()
            : base(
                "Parameters must have unique names",
                "This parameter's name must be unique within the command (comparison IS NOT case sensitive).")
        {
        }

        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
                return;

            var property = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
            if (property is null)
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
                .Where(m => !m.Equals(property, SymbolEqualityComparer.Default))
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
                    context.ReportDiagnostic(CreateDiagnostic(propertyDeclaration.GetLocation()));
                }
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.PropertyDeclaration);
        }
    }
}