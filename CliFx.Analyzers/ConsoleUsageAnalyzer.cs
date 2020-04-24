using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConsoleUsageAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor.CliFx0001);

        private static void CheckSystemConsoleUsage(SyntaxNodeAnalysisContext context)
        {
            // Invocation: Console.Error.WriteLine(...)
            if (!(context.Node is InvocationExpressionSyntax invocationSyntax))
                return;

            // Type identifier: Console
            var typeIdentifierSyntax = invocationSyntax
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .FirstOrDefault();

            if (typeIdentifierSyntax == null)
                return;

            // Type: System.Console
            if (!(context.SemanticModel.GetSymbolInfo(typeIdentifierSyntax).Symbol is INamedTypeSymbol namedTypeSymbol))
                return;

            // Is it System.Console?
            var isSystemConsole = KnownSymbols.IsSystemConsole(namedTypeSymbol);
            if (!isSystemConsole)
                return;

            // Is IConsole available in the context as a viable alternative?
            var isConsoleInterfaceAvailable = invocationSyntax
                .Ancestors()
                .OfType<MethodDeclarationSyntax>()
                .SelectMany(m => m.ParameterList.Parameters)
                .Select(p => p.Type)
                .Select(t => context.SemanticModel.GetSymbolInfo(t).Symbol)
                .Where(s => s != null)
                .Any(KnownSymbols.IsConsoleInterface!);

            if (!isConsoleInterfaceAvailable)
                return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor.CliFx0001, invocationSyntax.GetLocation()));
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(CheckSystemConsoleUsage, SyntaxKind.InvocationExpression);
        }
    }
}