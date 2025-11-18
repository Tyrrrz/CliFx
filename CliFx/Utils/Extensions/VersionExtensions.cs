using System;

namespace CliFx.Utils.Extensions;

internal static class VersionExtensions
{
    extension(Version version)
    {
        public string ToSemanticString() =>
            version.Revision <= 0 ? version.ToString(3) : version.ToString();
    }
}
