// ReSharper disable CheckNamespace

#if NETSTANDARD2_0
internal static class StringPolyfills
{
    public static bool StartsWith(this string str, char c) =>
        str.Length > 0 && str[0] == c;

    public static bool EndsWith(this string str, char c) =>
        str.Length > 0 && str[^1] == c;
}
#endif