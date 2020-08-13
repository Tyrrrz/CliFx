namespace CliFx.Analyzers
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConsoleUsageAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            DiagnosticDescriptors.CliFx0100
        );

        private static bool IsSystemConsoleInvocation(
            SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax invocationSyntax)
        {
            // Get the method member access (Console.WriteLine or Console.Error.WriteLine)
            if (!(invocationSyntax.Expression is MemberAccessExpressionSyntax memberAccessSyntax))
                return false;

            // Get the semantic model for the invoked method
            if (!(context.SemanticModel.GetSymbolInfo(memberAccessSyntax).Symbol is IMethodSymbol methodSymbol))
                return false;

            // Check if contained within System.Console
            if (KnownSymbols.IsSystemConsole(methodSymbol.ContainingType))
                return true;

            // In case with Console.Error.WriteLine that wouldn't work, we need to check parent member access too
            if (!(memberAccessSyntax.Expression is MemberAccessExpressionSyntax parentMemberAccessSyntax))
                return false;

            // Get the semantic model for the parent member
            if (!(context.SemanticModel.GetSymbolInfo(parentMemberAccessSyntax).Symbol is IPropertySymbol propertySymbol))
                return false;

            // Check if contained within System.Console
            if (KnownSymbols.IsSystemConsole(propertySymbol.ContainingType))
                return true;

            return false;
        }

        private static void CheckSystemConsoleUsage(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is InvocationExpressionSyntax invocationSyntax))
                return;

            if (!IsSystemConsoleInvocation(context, invocationSyntax))
                return;

            // Check if IConsole is available in the scope as a viable alternative
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

            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.CliFx0100, invocationSyntax.GetLocation()));
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(CheckSystemConsoleUsage, SyntaxKind.InvocationExpression);
        }
    }
}