using System.Collections.Immutable;
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
    public class SystemConsoleShouldBeAvoidedAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor DiagnosticDescriptor { get; } = new(
            "CliFx_" + nameof(SystemConsoleShouldBeAvoidedAnalyzer).TrimEnd("Analyzer"),
            "Avoid referencing `System.Console` inside a command",
            "Use the provided `CliFx.IConsole` abstraction instead of `System.Console` to ensure that the command can be tested in isolation.",
            "CliFx", DiagnosticSeverity.Warning, true
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(DiagnosticDescriptor);

        private static MemberAccessExpressionSyntax? TryGetSystemConsoleMemberAccess(
            SyntaxNodeAnalysisContext context,
            SyntaxNode node)
        {
            var currentNode = node;

            while (currentNode is MemberAccessExpressionSyntax memberAccess)
            {
                var symbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol;

                if (symbol is not null && KnownSymbols.IsSystemConsole(symbol.ContainingType))
                {
                    return memberAccess;
                }

                // Get inner expression, which may be another member access expression.
                // Example: System.Console.Error
                //          ~~~~~~~~~~~~~~          <- inner member access expression
                //          --------------------    <- outer member access expression
                currentNode = memberAccess.Expression;
            }

            return null;
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            // Try to get a member access on System.Console in the current expression,
            // or in any of its inner expressions.
            var systemConsoleMemberAccess = TryGetSystemConsoleMemberAccess(context, context.Node);
            if (systemConsoleMemberAccess is null)
                return;

            // Check if IConsole is available in scope as an alternative to System.Console
            var isConsoleInterfaceAvailable = context.Node
                .Ancestors()
                .OfType<MethodDeclarationSyntax>()
                .SelectMany(m => m.ParameterList.Parameters)
                .Select(p => p.Type)
                .Select(t => context.SemanticModel.GetSymbolInfo(t).Symbol)
                .Where(s => s is not null)
                .Any(KnownSymbols.IsCliFxConsoleInterface);

            if (isConsoleInterfaceAvailable)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptor,
                    systemConsoleMemberAccess.GetLocation()
                ));
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.SimpleMemberAccessExpression);
        }
    }
}