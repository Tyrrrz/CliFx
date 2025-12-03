using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration.Utils.Extensions;

internal static class RoslynExtensions
{
    extension(ISymbol symbol)
    {
        public bool DisplayNameMatches(string name) =>
            string.Equals(
                // Fully qualified name, without `global::`
                symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat),
                name,
                StringComparison.Ordinal
            );
    }

    extension(AttributeData attribute)
    {
        public T GetNamedArgumentValue<T>(string name, T defaultValue = default) =>
            attribute.NamedArguments.FirstOrDefault(i => i.Key == name).Value.Value is T valueAsT
                ? valueAsT
                : defaultValue;

        public IReadOnlyList<T> GetNamedArgumentValues<T>(string name)
            where T : class =>
            attribute.NamedArguments.FirstOrDefault(i => i.Key == name).Value.Values.CastArray<T>();
    }

    public static IncrementalValuesProvider<T> WhereNotNull<T>(
        this IncrementalValuesProvider<T?> values
    )
        where T : class => values.Where(i => i is not null);
}
