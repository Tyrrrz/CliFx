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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            DiagnosticDescriptors.CliFx0100
        );

        private static bool IsSystemConsoleInvocation(
            SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax invocationSyntax)
        {
            if (invocationSyntax.Expression is MemberAccessExpressionSyntax memberAccessSyntax &&
                context.SemanticModel.GetSymbolInfo(memberAccessSyntax).Symbol is IMethodSymbol methodSymbol)
            {
                // Direct call to System.Console (e.g. System.Console.WriteLine())
                if (KnownSymbols.IsSystemConsole(methodSymbol.ContainingType))
                {
                    return true;
                }

                // Indirect call to System.Console (e.g. System.Console.Error.WriteLine())
                if (memberAccessSyntax.Expression is MemberAccessExpressionSyntax parentMemberAccessSyntax &&
                    context.SemanticModel.GetSymbolInfo(parentMemberAccessSyntax).Symbol is IPropertySymbol propertySymbol)
                {
                    return KnownSymbols.IsSystemConsole(propertySymbol.ContainingType);
                }
            }

            return false;
        }

        private static void CheckSystemConsoleUsage(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is InvocationExpressionSyntax invocationSyntax &&
                IsSystemConsoleInvocation(context, invocationSyntax))
            {
                // Check if IConsole is available in scope as alternative to System.Console
                var isConsoleInterfaceAvailable = invocationSyntax
                    .Ancestors()
                    .OfType<MethodDeclarationSyntax>()
                    .SelectMany(m => m.ParameterList.Parameters)
                    .Select(p => p.Type)
                    .Select(t => context.SemanticModel.GetSymbolInfo(t).Symbol)
                    .Where(s => s != null)
                    .Any(KnownSymbols.IsConsoleInterface!);

                if (isConsoleInterfaceAvailable)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.CliFx0100,
                        invocationSyntax.GetLocation()
                    ));
                }
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(CheckSystemConsoleUsage, SyntaxKind.InvocationExpression);
        }
    }
}