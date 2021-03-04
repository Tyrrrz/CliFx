using System.Linq;
using CliFx.Analyzers.ObjectModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    public class ArgumentMustHaveValidConverterAnalyzer : AnalyzerBase
    {
        public ArgumentMustHaveValidConverterAnalyzer()
            : base(
                "Parameter converter type must implement `IArgumentValueConverter`",
                "Type must implement `CliFx.IArgumentValueConverter` in order to be used as converter.")
        {
        }

        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
                return;

            var property = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
            if (property is null)
                return;

            var argument =
                (ICommandArgumentSymbol)
                CommandParameterSymbol.TryResolve(property) ??
                CommandOptionSymbol.TryResolve(property);

            if (argument is null)
                return;

            if (argument.ConverterType is null)
                return;

            if (!argument.ConverterType.AllInterfaces.Any(KnownSymbols.IsArgumentValueConverterInterface))
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