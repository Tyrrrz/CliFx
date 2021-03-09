using System.Linq;
using CliFx.Analyzers.ObjectModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    public class ParameterMustBeSingleIfNonScalarAnalyzer : AnalyzerBase
    {
        public ParameterMustBeSingleIfNonScalarAnalyzer()
            : base(
                "Parameters of non-scalar types are limited to one per command",
                "This parameter has a non-scalar type so it must be the only such parameter in the command.")
        {
        }

        private static bool IsScalar(ITypeSymbol type) =>
            KnownSymbols.IsSystemString(type) ||
            !type.AllInterfaces
                .Select(i => i.ConstructedFrom)
                .Any(KnownSymbols.IsSystemCollectionsGenericIEnumerable);

        private void Analyze(SyntaxNodeAnalysisContext context)
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