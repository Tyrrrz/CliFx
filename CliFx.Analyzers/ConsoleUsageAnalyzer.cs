using System;
using System.Collections.Immutable;
using System.Linq;
using CliFx.Analyzers.Internal;
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
            if (!(context.Node is InvocationExpressionSyntax invocationSyntax))
                return;

            if (!(context.SemanticModel.GetSymbolInfo(invocationSyntax).Symbol is IMethodSymbol methodSymbol))
                return;

            var isSystemConsoleMethodCalled =
                KnownSymbols.IsSystemConsole(methodSymbol.ContainingType);

            if (!isSystemConsoleMethodCalled)
                return;

            var isConsoleInterfaceAvailable = invocationSyntax.GetAncestors()
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