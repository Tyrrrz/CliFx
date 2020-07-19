using System;

namespace CliFx.Internal.Extensions
{
    internal static class VersionExtensions
    {
        public static string ToSemanticString(this Version version) =>
            version.Revision <= 0 ? version.ToString(3) : version.ToString();
    }
}