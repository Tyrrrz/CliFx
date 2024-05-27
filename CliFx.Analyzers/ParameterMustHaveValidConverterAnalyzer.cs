using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ParameterMustHaveValidConverterAnalyzer()
    : AnalyzerBase(
        $"Parameter converters must derive from `{SymbolNames.CliFxBindingConverterClass}`",
        $"Converter specified for this parameter must derive from a compatible `{SymbolNames.CliFxBindingConverterClass}`."
    )
{
    private void Analyze(
        SyntaxNodeAnalysisContext context,
        PropertyDeclarationSyntax propertyDeclaration,
        IPropertySymbol property
    )
    {
        var parameter = CommandParameterSymbol.TryResolve(property);
        if (parameter is null)
            return;

        if (parameter.ConverterType is null)
            return;

        var converterValueType = parameter
            .ConverterType.GetBaseTypes()
            .FirstOrDefault(t =>
                t.ConstructedFrom.DisplayNameMatches(SymbolNames.CliFxBindingConverterClass)
            )
            ?.TypeArguments.FirstOrDefault();

        // Value returned by the converter must be assignable to the property type
        var isCompatible =
            converterValueType is not null
            && (
                parameter.IsScalar()
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
