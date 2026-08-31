using Microsoft.CodeAnalysis.CSharp;

namespace CliFx.Generators.Utils;

internal static class CSharp
{
    public static string Encode(string? str) =>
        str is not null ? SymbolDisplay.FormatLiteral(str, true) : "null";

    public static string Encode(char? ch) =>
        ch is not null ? SymbolDisplay.FormatLiteral(ch.Value, true) : "null";
}
