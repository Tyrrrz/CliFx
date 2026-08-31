namespace CliFx.Generators.Utils;

internal static class CSharp
{
    public static string Encode(string? str) =>
        str is null ? "null" : Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(str, true);

    public static string Encode(char? c) => c is null ? "null" : $"'{c}'";
}
