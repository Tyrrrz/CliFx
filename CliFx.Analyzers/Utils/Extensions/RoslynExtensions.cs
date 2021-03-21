using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CliFx.Analyzers.Utils.Extensions
{
    internal static class RoslynExtensions
    {
        public static bool DisplayNameMatches(this ISymbol symbol, string name) =>
            string.Equals(symbol.ToDisplayString(), name, StringComparison.Ordinal);

        public static void HandleClassDeclaration(
            this AnalysisContext analysisContext,
            Action<SyntaxNodeAnalysisContext, ClassDeclarationSyntax, ITypeSymbol> handler)
        {
            analysisContext.RegisterSyntaxNodeAction(ctx =>
            {
                if (ctx.Node is not ClassDeclarationSyntax classDeclaration)
                    return;

                var type = ctx.SemanticModel.GetDeclaredSymbol(classDeclaration);
                if (type is null)
                    return;

                handler(ctx, classDeclaration, type);
            }, SyntaxKind.ClassDeclaration);
        }

        public static void HandlePropertyDeclaration(
            this AnalysisContext analysisContext,
            Action<SyntaxNodeAnalysisContext, PropertyDeclarationSyntax, IPropertySymbol> handler)
        {
            analysisContext.RegisterSyntaxNodeAction(ctx =>
            {
                if (ctx.Node is not PropertyDeclarationSyntax propertyDeclaration)
                    return;

                var property = ctx.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
                if (property is null)
                    return;

                handler(ctx, propertyDeclaration, property);
            }, SyntaxKind.PropertyDeclaration);
        }
    }
}