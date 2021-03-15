using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ArgumentMustHaveValidConverterAnalyzer : AnalyzerBase
    {
        public ArgumentMustHaveValidConverterAnalyzer()
            : base(
                $"Parameter and option converters must derive from `{SymbolNames.CliFxArgumentConverterClass}`",
                $"Converter specified for this parameter or option must derive from `{SymbolNames.CliFxArgumentConverterClass}`.")
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
                (ICommandMemberSymbol)
                CommandParameterSymbol.TryResolve(property) ??
                CommandOptionSymbol.TryResolve(property);

            if (argument is null)
                return;

            if (argument.ConverterType is null)
                return;

            // We check against an internal interface because checking against a generic class is a pain
            var converterImplementsInterface = argument
                .ConverterType
                .AllInterfaces
                .Any(s => s.DisplayNameMatches(SymbolNames.CliFxArgumentConverterInterface));

            if (!converterImplementsInterface)
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