using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

    public static T GetNamedArgumentValue<T>(
        this AttributeData attribute,
        string name,
        T defaultValue = default
    ) =>
        attribute.NamedArguments.FirstOrDefault(i => i.Key == name).Value.Value is T valueAsT
            ? valueAsT
            : defaultValue;

    public static IReadOnlyList<T> GetNamedArgumentValues<T>(
        this AttributeData attribute,
        string name
    )
        where T : class =>
        attribute.NamedArguments.FirstOrDefault(i => i.Key == name).Value.Values.CastArray<T>();

    public static IncrementalValuesProvider<T> WhereNotNull<T>(
        this IncrementalValuesProvider<T?> values
    )
        where T : class => values.Where(i => i is not null);
}
