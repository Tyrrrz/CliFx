namespace CliFx.Analyzers.Internal
{
    using System;
    using Microsoft.CodeAnalysis;

    internal static class RoslynExtensions
    {
        public static bool DisplayNameMatches(this ISymbol symbol, string name)
        {
            return string.Equals(symbol.ToDisplayString(), name, StringComparison.Ordinal);
        }
    }
}