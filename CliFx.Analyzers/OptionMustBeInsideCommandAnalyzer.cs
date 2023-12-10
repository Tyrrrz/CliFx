using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OptionMustBeInsideCommandAnalyzer()
    : AnalyzerBase(
        "Options must be defined inside commands",
        $"This option must be defined inside a class that implements `{SymbolNames.CliFxCommandInterface}`."
    )
{
    private void Analyze(
        SyntaxNodeAnalysisContext context,
        PropertyDeclarationSyntax propertyDeclaration,
        IPropertySymbol property
    )
    {
        if (property.ContainingType is null)
            return;

        if (property.ContainingType.IsAbstract)
            return;

        if (!CommandOptionSymbol.IsOptionProperty(property))
            return;

        var isInsideCommand = property
            .ContainingType
            .AllInterfaces
            .Any(i => i.DisplayNameMatches(SymbolNames.CliFxCommandInterface));

        if (!isInsideCommand)
        {
            context.ReportDiagnostic(
                CreateDiagnostic(propertyDeclaration.Identifier.GetLocation())
            );
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);
        context.HandlePropertyDeclaration(Analyze);
    }
}
