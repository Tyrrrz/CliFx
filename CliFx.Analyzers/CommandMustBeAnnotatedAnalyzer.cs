using System.Collections.Immutable;
using System.Linq;
using CliFx.Analyzers.ObjectModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    // TODO: merge with CommandMustImplementInterfaceAnalyzer?
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CommandMustBeAnnotatedAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor DiagnosticDescriptor { get; } = new(
            "CliFx_CommandMustBeAnnotated",
            "Command must be annotated with `CommandAttribute`",
            "Type must be annotated with `CliFx.CommandAttribute` in order to be a valid command.",
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

            // Ignore abstract classes, because they may be used to define
            // base implementations for commands, in which case the command
            // attribute doesn't make sense.
            if (type.IsAbstract)
                return;

            var implementsCommandInterface = type
                .AllInterfaces
                .Any(KnownSymbols.IsCommandInterface);

            var hasCommandAttribute = type
                .GetAttributes()
                .Select(a => a.AttributeClass)
                .Any(KnownSymbols.IsCommandAttribute);

            // If the interface is implemented, but the attribute is missing,
            // then it's very likely a user error.
            if (implementsCommandInterface && !hasCommandAttribute)
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