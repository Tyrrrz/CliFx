using System.Linq;
using CliFx.Analyzers.ObjectModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    public class ArgumentMustBeInsideCommandAnalyzer : AnalyzerBase
    {
        public ArgumentMustBeInsideCommandAnalyzer()
            : base(
                "Parameter must be defined inside a command",
                "Specified parameter is defined in a type which is not a command.")
        {
        }

        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
                return;

            var property = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
            if (property is null)
                return;

            if (property.ContainingType.IsAbstract)
                return;

            if (!CommandParameterSymbol.IsParameterProperty(property) &&
                !CommandOptionSymbol.IsOptionProperty(property))
                return;

            var isInsideCommand = property
                .ContainingType
                .AllInterfaces
                .Any(KnownSymbols.IsCommandInterface);

            if (!isInsideCommand)
            {
                context.ReportDiagnostic(CreateDiagnostic(propertyDeclaration.GetLocation()));
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.PropertyDeclaration);
        }
    }
}