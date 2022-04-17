using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SystemConsoleShouldBeAvoidedAnalyzer : AnalyzerBase
{
    public SystemConsoleShouldBeAvoidedAnalyzer()
        : base(
            $"Avoid calling `System.Console` where `{SymbolNames.CliFxConsoleInterface}` is available",
            $"Use the provided `{SymbolNames.CliFxConsoleInterface}` abstraction instead of `System.Console` to ensure that the command can be tested in isolation.",
            DiagnosticSeverity.Warning)
    {
    }

    private MemberAccessExpressionSyntax? TryGetSystemConsoleMemberAccess(
        SyntaxNodeAnalysisContext context,
        SyntaxNode node)
    {
        var currentNode = node;

        while (currentNode is MemberAccessExpressionSyntax memberAccess)
        {
            var member = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol;

            if (member?.ContainingType?.DisplayNameMatches("System.Console") == true)
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

    private void Analyze(SyntaxNodeAnalysisContext context)
    {
        // Try to get a member access on System.Console in the current expression,
        // or in any of its inner expressions.
        var systemConsoleMemberAccess = TryGetSystemConsoleMemberAccess(context, context.Node);
        if (systemConsoleMemberAccess is null)
            return;

        // Check if IConsole is available in scope as an alternative to System.Console
        var isConsoleInterfaceAvailable = context
            .Node
            .Ancestors()
            .OfType<MethodDeclarationSyntax>()
            .SelectMany(m => m.ParameterList.Parameters)
            .Select(p => p.Type)
            .Select(t => context.SemanticModel.GetSymbolInfo(t).Symbol)
            .Where(s => s is not null)
            .Any(s => s.DisplayNameMatches(SymbolNames.CliFxConsoleInterface));

        if (isConsoleInterfaceAvailable)
        {
            context.ReportDiagnostic(
                CreateDiagnostic(systemConsoleMemberAccess.GetLocation())
            );
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.SimpleMemberAccessExpression);
    }
}