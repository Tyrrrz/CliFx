using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class OptionMustHaveValidConverterAnalyzer()
    : AnalyzerBase(
        $"Option converters must derive from `{SymbolNames.CliFxBindingConverterClass}`",
        $"Converter specified for this option must derive from a compatible `{SymbolNames.CliFxBindingConverterClass}`."
    )
{
    private void Analyze(
        SyntaxNodeAnalysisContext context,
        PropertyDeclarationSyntax propertyDeclaration,
        IPropertySymbol property
    )
    {
        var option = CommandOptionSymbol.TryResolve(property);
        if (option is null)
            return;

        if (option.ConverterType is null)
            return;

        var converterValueType = option
            .ConverterType.GetBaseTypes()
            .FirstOrDefault(t =>
                t.ConstructedFrom.DisplayNameMatches(SymbolNames.CliFxBindingConverterClass)
            )
            ?.TypeArguments.FirstOrDefault();

        // Value returned by the converter must be assignable to the property type
        var isCompatible =
            converterValueType is not null
            && (
                option.IsScalar()
                    // Scalar
                    ? context.Compilation.IsAssignable(converterValueType, property.Type)
                    // Non-scalar (assume we can handle all IEnumerable types for simplicity)
                    : property.Type.TryGetEnumerableUnderlyingType() is { } enumerableUnderlyingType
                        && context.Compilation.IsAssignable(
                            converterValueType,
                            enumerableUnderlyingType
                        )
            );

        if (!isCompatible)
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
