﻿using System.Linq;
using CliFx.Analyzers.ObjectModel;
using CliFx.Analyzers.Utils.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class OptionMustHaveValidConverterAnalyzer : AnalyzerBase
    {
        public OptionMustHaveValidConverterAnalyzer()
            : base(
                $"Option converters must derive from `{SymbolNames.CliFxBindingConverterClass}`",
                $"Converter specified for this option must derive from `{SymbolNames.CliFxBindingConverterClass}`.")
        {
        }

        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
                return;

            var property = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
            if (property is null)
                return;

            var option = CommandOptionSymbol.TryResolve(property);
            if (option is null)
                return;

            if (option.ConverterType is null)
                return;

            // We check against an internal interface because checking against a generic class is a pain
            var converterImplementsInterface = option
                .ConverterType
                .AllInterfaces
                .Any(s => s.DisplayNameMatches(SymbolNames.CliFxBindingConverterInterface));

            if (!converterImplementsInterface)
            {
                context.ReportDiagnostic(CreateDiagnostic(propertyDeclaration.GetLocation()));
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            base.Initialize(context);
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.PropertyDeclaration);
        }
    }
}