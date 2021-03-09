using System.Linq;
using CliFx.Analyzers.ObjectModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    public class ParameterMustBeLastIfNonScalarAnalyzer : AnalyzerBase
    {
        public ParameterMustBeLastIfNonScalarAnalyzer()
            : base(
                "Parameters of non-scalar types must be last in order",
                "This parameter has a non-scalar type so it must be last in order (its order must be highest within the command).")
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

            if (IsScalar(property.Type))
                return;

            var parameter = CommandParameterSymbol.TryResolve(property);
            if (parameter is null)
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
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.PropertyDeclaration);
        }
    }
}