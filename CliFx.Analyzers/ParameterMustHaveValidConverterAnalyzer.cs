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
    public class ParameterMustHaveValidConverterAnalyzer : AnalyzerBase
    {
        public ParameterMustHaveValidConverterAnalyzer()
            : base(
                $"Parameter converters must derive from `{SymbolNames.CliFxBindingConverterClass}`",
                $"Converter specified for this parameter must derive from `{SymbolNames.CliFxBindingConverterClass}`.")
        {
        }

        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
                return;

            var property = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
            if (property is null)
                return;

            var parameter = CommandParameterSymbol.TryResolve(property);
            if (parameter is null)
                return;

            if (parameter.ConverterType is null)
                return;

            // We check against an internal interface because checking against a generic class is a pain
            var converterImplementsInterface = parameter
                .ConverterType
                .AllInterfaces
                .Any(s => s.DisplayNameMatches(SymbolNames.CliFxBindingConverterInterface));

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