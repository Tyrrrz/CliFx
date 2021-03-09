using System.Linq;
using CliFx.Analyzers.ObjectModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    public class CommandMustImplementInterfaceAnalyzer : AnalyzerBase
    {
        public CommandMustImplementInterfaceAnalyzer()
            : base(
                $"Commands must implement `{KnownSymbols.CliFxCommandInterface}` interface",
                $"This type must implement `{KnownSymbols.CliFxCommandInterface}` interface in order to be a valid command.")
        {
        }

        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not ClassDeclarationSyntax classDeclaration)
                return;

            var type = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
            if (type is null)
                return;

            var hasCommandAttribute = type
                .GetAttributes()
                .Select(a => a.AttributeClass)
                .Any(KnownSymbols.IsCommandAttribute);

            var implementsCommandInterface = type
                .AllInterfaces
                .Any(KnownSymbols.IsCommandInterface);

            // If the attribute is present, but the interface is not implemented,
            // it's very likely a user error.
            if (hasCommandAttribute && !implementsCommandInterface)
            {
                context.ReportDiagnostic(CreateDiagnostic(classDeclaration.GetLocation()));
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ClassDeclaration);
        }
    }
}