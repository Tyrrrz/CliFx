namespace CliFx.Generators.Utils;

internal static class CSharp
{
    public static string Escape(string str) => str.Replace("\\", @"\\").Replace("\"", "\\\"");

    public static string Encode(string? str) => str is null ? "null" : $"\"{Escape(str)}\"";
}
