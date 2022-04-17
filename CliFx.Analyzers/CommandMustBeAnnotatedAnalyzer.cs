using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CommandMustBeAnnotatedAnalyzer : AnalyzerBase
{
    public CommandMustBeAnnotatedAnalyzer()
        : base(
            $"Commands must be annotated with `{SymbolNames.CliFxCommandAttribute}`",
            $"This type must be annotated with `{SymbolNames.CliFxCommandAttribute}` in order to be a valid command.")
    {
    }

    private void Analyze(
        SyntaxNodeAnalysisContext context,
        ClassDeclarationSyntax classDeclaration,
        ITypeSymbol type)
    {
        // Ignore abstract classes, because they may be used to define
        // base implementations for commands, in which case the command
        // attribute doesn't make sense.
        if (type.IsAbstract)
            return;

        var implementsCommandInterface = type
            .AllInterfaces
            .Any(i => i.DisplayNameMatches(SymbolNames.CliFxCommandInterface));

        var hasCommandAttribute = type
            .GetAttributes()
            .Select(a => a.AttributeClass)
            .Any(c => c.DisplayNameMatches(SymbolNames.CliFxCommandAttribute));

        // If the interface is implemented, but the attribute is missing,
        // then it's very likely a user error.
        if (implementsCommandInterface && !hasCommandAttribute)
        {
            context.ReportDiagnostic(
                CreateDiagnostic(classDeclaration.Identifier.GetLocation())
            );
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);
        context.HandleClassDeclaration(Analyze);
    }
}