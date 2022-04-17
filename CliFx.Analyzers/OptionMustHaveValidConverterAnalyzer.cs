using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OptionMustHaveValidConverterAnalyzer : AnalyzerBase
{
    public OptionMustHaveValidConverterAnalyzer()
        : base(
            $"Option converters must derive from `{SymbolNames.CliFxBindingConverterClass}`",
            $"Converter specified for this option must derive from a compatible `{SymbolNames.CliFxBindingConverterClass}`.")
    {
    }

    private void Analyze(
        SyntaxNodeAnalysisContext context,
        PropertyDeclarationSyntax propertyDeclaration,
        IPropertySymbol property)
    {
        var option = CommandOptionSymbol.TryResolve(property);
        if (option is null)
            return;

        if (option.ConverterType is null)
            return;

        var converterValueType = option
            .ConverterType
            .GetBaseTypes()
            .FirstOrDefault(t => t.ConstructedFrom.DisplayNameMatches(SymbolNames.CliFxBindingConverterClass))?
            .TypeArguments
            .FirstOrDefault();

        // Value returned by the converter must be assignable to the property type
        if (converterValueType is null || !property.Type.IsAssignableFrom(converterValueType))
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