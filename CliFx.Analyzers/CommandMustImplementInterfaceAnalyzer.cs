using System.Collections.Immutable;
using System.Linq;
using CliFx.Analyzers.ObjectModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CommandMustImplementInterfaceAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor DiagnosticDescriptor { get; } = new(
            "CliFx_CommandMustImplementInterface",
            "Command must implement the ICommand interface",
            "The type must implement the `CliFx.ICommand` interface in order to be a valid command.",
            "CliFx", DiagnosticSeverity.Error, true
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(DiagnosticDescriptor);

        private static void Analyze(SyntaxNodeAnalysisContext context)
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
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptor,
                    classDeclaration.GetLocation()
                ));
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.ClassDeclaration);
        }
    }
}