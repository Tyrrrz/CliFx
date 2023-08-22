using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ParameterMustHaveValidValidatorsAnalyzer : AnalyzerBase
{
    public ParameterMustHaveValidValidatorsAnalyzer()
        : base(
            $"Parameter validators must derive from `{SymbolNames.CliFxBindingValidatorClass}`",
            $"Each validator specified for this parameter must derive from a compatible `{SymbolNames.CliFxBindingValidatorClass}`."
        ) { }

    private void Analyze(
        SyntaxNodeAnalysisContext context,
        PropertyDeclarationSyntax propertyDeclaration,
        IPropertySymbol property
    )
    {
        var parameter = CommandParameterSymbol.TryResolve(property);
        if (parameter is null)
            return;

        foreach (var validatorType in parameter.ValidatorTypes)
        {
            var validatorValueType = validatorType
                .GetBaseTypes()
                .FirstOrDefault(
                    t =>
                        t.ConstructedFrom.DisplayNameMatches(SymbolNames.CliFxBindingValidatorClass)
                )
                ?.TypeArguments.FirstOrDefault();

            // Value passed to the validator must be assignable from the property type
            var isCompatible =
                validatorValueType is not null
                && context.Compilation.IsAssignable(property.Type, validatorValueType);

            if (!isCompatible)
            {
                context.ReportDiagnostic(
                    CreateDiagnostic(propertyDeclaration.Identifier.GetLocation())
                );

                // No need to report multiple identical diagnostics on the same node
                break;
            }
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);
        context.HandlePropertyDeclaration(Analyze);
    }
}
