using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CommandMustImplementInterfaceAnalyzer()
    : AnalyzerBase(
        $"Commands must implement `{SymbolNames.CliFxCommandInterface}` interface",
        $"This type must implement `{SymbolNames.CliFxCommandInterface}` interface in order to be a valid command."
    )
{
    private void Analyze(
        SyntaxNodeAnalysisContext context,
        ClassDeclarationSyntax classDeclaration,
        ITypeSymbol type
    )
    {
        var hasCommandAttribute = type.GetAttributes()
            .Select(a => a.AttributeClass)
            .Any(c => c.DisplayNameMatches(SymbolNames.CliFxCommandAttribute));

        var implementsCommandInterface = type.AllInterfaces.Any(
            i => i.DisplayNameMatches(SymbolNames.CliFxCommandInterface)
        );

        // If the attribute is present, but the interface is not implemented,
        // it's very likely a user error.
        if (hasCommandAttribute && !implementsCommandInterface)
        {
            context.ReportDiagnostic(CreateDiagnostic(classDeclaration.Identifier.GetLocation()));
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);
        context.HandleClassDeclaration(Analyze);
    }
}
