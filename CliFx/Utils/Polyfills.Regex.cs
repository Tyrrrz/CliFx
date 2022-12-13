// ReSharper disable CheckNamespace

#if NETSTANDARD2_0
using System.Linq;
using System.Text.RegularExpressions;

internal static class RegexPolyfills
{
    public static Match[] ToArray(this MatchCollection matches) =>
        matches.Cast<Match>().ToArray();
}
#endif