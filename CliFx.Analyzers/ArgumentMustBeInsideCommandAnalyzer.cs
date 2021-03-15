using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    // TODO: rename argument to something else
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ArgumentMustBeInsideCommandAnalyzer : AnalyzerBase
    {
        public ArgumentMustBeInsideCommandAnalyzer()
            : base(
                "Parameters and options must be defined inside commands",
                $"This parameter or option must be defined inside a class that implements `{SymbolNames.CliFxCommandInterface}`.")
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
                .Any(s => s.DisplayNameMatches(SymbolNames.CliFxCommandInterface));

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