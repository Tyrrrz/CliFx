using System.Collections.Immutable;
using System.Linq;
using CliFx.Analyzers.ObjectModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ParameterOrderMustBeUniqueAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor DiagnosticDescriptor { get; } = new(
            "CliFx_ParameterOrderMustBeUnique",
            "Parameter order must be unique",
            "The specified order is equal to another parameter's order in the same command.",
            "CliFx", DiagnosticSeverity.Error, true
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(DiagnosticDescriptor);

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
                return;

            var property = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
            if (property is null)
                return;

            var parameterSchema = CommandParameterSymbol.TryResolve(property);
            if (parameterSchema is null)
                return;

            var otherProperties = property
                .ContainingType
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(m => !m.Equals(property, SymbolEqualityComparer.Default))
                .ToArray();

            foreach (var otherProperty in otherProperties)
            {
                var otherParameterSchema = CommandParameterSymbol.TryResolve(otherProperty);
                if (otherParameterSchema is null)
                    continue;

                if (parameterSchema.Order == otherParameterSchema.Order)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptor,
                        propertyDeclaration.GetLocation()
                    ));
                }
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.PropertyDeclaration);
        }
    }
}