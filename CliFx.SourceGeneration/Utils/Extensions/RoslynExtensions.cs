using System;
using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.Utils.Extensions;

internal static class RoslynExtensions
{
    public static bool DisplayNameMatches(this ISymbol symbol, string name) =>
        string.Equals(
            // Fully qualified name, without `global::`
            symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
            name,
            StringComparison.Ordinal
        );

    public static IncrementalValuesProvider<T> WhereNotNull<T>(
        this IncrementalValuesProvider<T?> values
    )
        where T : class => values.Where(i => i is not null);
}
