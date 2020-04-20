using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CliFx.Analyzers.Internal
{
    internal static class RoslynExtensions
    {
        public static bool DisplayNameMatches(this ISymbol symbol, string name) =>
            string.Equals(symbol.ToDisplayString(), name, StringComparison.Ordinal);

        public static IEnumerable<SyntaxNode> GetAncestors(this SyntaxNode syntaxNode)
        {
            var current = syntaxNode;
            while (current != null)
            {
                yield return current;
                current = current.Parent;
            }
        }
    }
}