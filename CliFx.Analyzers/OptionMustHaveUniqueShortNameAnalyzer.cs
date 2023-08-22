using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OptionMustHaveUniqueShortNameAnalyzer : AnalyzerBase
{
    public OptionMustHaveUniqueShortNameAnalyzer()
        : base(
            "Options must have unique short names",
            "This option's short name must be unique within the command (comparison IS case sensitive). "
                + "Specified short name: `{0}` "
                + "Property bound to another option with the same short name: `{1}`."
        ) { }

    private void Analyze(
        SyntaxNodeAnalysisContext context,
        PropertyDeclarationSyntax propertyDeclaration,
        IPropertySymbol property
    )
    {
        if (property.ContainingType is null)
            return;

        var option = CommandOptionSymbol.TryResolve(property);
        if (option is null)
            return;

        if (option.ShortName is null)
            return;

        var otherProperties = property.ContainingType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(m => !m.Equals(property))
            .ToArray();

        foreach (var otherProperty in otherProperties)
        {
            var otherOption = CommandOptionSymbol.TryResolve(otherProperty);
            if (otherOption is null)
                continue;

            if (otherOption.ShortName is null)
                continue;

            if (option.ShortName == otherOption.ShortName)
            {
                context.ReportDiagnostic(
                    CreateDiagnostic(
                        propertyDeclaration.Identifier.GetLocation(),
                        option.ShortName,
                        otherProperty.Name
                    )
                );
            }
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);
        context.HandlePropertyDeclaration(Analyze);
    }
}
