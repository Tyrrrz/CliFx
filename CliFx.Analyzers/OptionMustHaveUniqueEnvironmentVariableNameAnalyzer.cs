using System;
using System.Linq;
using CliFx.Analyzers.ObjectModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    public class OptionMustHaveUniqueEnvironmentVariableNameAnalyzer : AnalyzerBase
    {
        public OptionMustHaveUniqueEnvironmentVariableNameAnalyzer()
            : base(
                "Option environment variable name must be unique within its command",
                "Option environment variable name must be unique within its command")
        {
        }

        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
                return;

            var property = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
            if (property is null)
                return;

            var option = CommandOptionSymbol.TryResolve(property);
            if (option is null)
                return;

            if (string.IsNullOrWhiteSpace(option.EnvironmentVariableName))
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

                if (string.IsNullOrWhiteSpace(otherOption.EnvironmentVariableName))
                    continue;

                if (string.Equals(option.EnvironmentVariableName, otherOption.EnvironmentVariableName,
                    StringComparison.OrdinalIgnoreCase))
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