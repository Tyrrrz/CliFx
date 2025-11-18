using System;
using System.IO;

namespace CliFx.Utils.Extensions;

internal static class PathExtensions
{
    private static StringComparer EqualityComparer { get; } =
        OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    extension(Path)
    {
        public static bool AreEqual(string path1, string path2)
        {
            static string Normalize(string path) =>
                Path.GetFullPath(path)
                    .Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return EqualityComparer.Equals(Normalize(path1), Normalize(path2));
        }
    }
}
