using System;

namespace CliFx.Utils.Extensions
{
    internal static class VersionExtensions
    {
        public static string ToSemanticString(this Version version) =>
            version.Revision <= 0 ? version.ToString(3) : version.ToString();
    }
}