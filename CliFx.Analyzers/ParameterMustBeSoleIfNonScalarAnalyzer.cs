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
    public class ParameterMustBeSoleIfNonScalarAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor DiagnosticDescriptor { get; } = new(
            "CliFx_ParameterMustBeSoleIfNonScalar",
            "Only one parameter per command can have non-scalar type",
            "Specified parameter is not the only non-scalar parameter in the command.",
            "CliFx", DiagnosticSeverity.Error, true
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(DiagnosticDescriptor);

        private static bool IsScalar(ITypeSymbol type) =>
            KnownSymbols.IsSystemString(type) ||
            !type.AllInterfaces
                .Select(i => i.ConstructedFrom)
                .Any(KnownSymbols.IsSystemCollectionsGenericIEnumerable);

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
                return;

            var property = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
            if (property is null)
                return;

            if (!CommandParameterSymbol.IsParameterProperty(property))
                return;

            if (IsScalar(property.Type))
                return;

            var otherProperties = property
                .ContainingType
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(m => !m.Equals(property, SymbolEqualityComparer.Default))
                .ToArray();

            foreach (var otherProperty in otherProperties)
            {
                if (!CommandParameterSymbol.IsParameterProperty(otherProperty))
                    continue;

                if (!IsScalar(otherProperty.Type))
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