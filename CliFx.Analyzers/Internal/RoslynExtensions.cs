using System;
using Microsoft.CodeAnalysis;

namespace CliFx.Analyzers.Internal
{
    internal static class RoslynExtensions
    {
        public static bool DisplayNameMatches(this ISymbol symbol, string name) =>
            string.Equals(symbol.ToDisplayString(), name, StringComparison.Ordinal);
    }
}