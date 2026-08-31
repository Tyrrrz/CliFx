using Microsoft.CodeAnalysis.CSharp;

namespace CliFx.Generators.Utils;

internal static class CSharp
{
    public static string Encode(string? str) =>
        str is null ? "null" : SymbolDisplay.FormatLiteral(str, true);

    public static string Encode(char? c) =>
        c is null ? "null" : SymbolDisplay.FormatLiteral(c.Value, true);
}
