using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CliFx.Utils;

internal static class PathEx
{
    private static StringComparer EqualityComparer { get; } =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    public static bool AreEqual(string path1, string path2)
    {
        static string Normalize(string path) => Path
            .GetFullPath(path)
            .Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return EqualityComparer.Equals(Normalize(path1), Normalize(path2));
    }
}