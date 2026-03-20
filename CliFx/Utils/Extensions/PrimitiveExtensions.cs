using System;

namespace CliFx.Utils.Extensions;

internal static class PrimitiveExtensions
{
    extension(bool)
    {
        public static bool ParseOrDefault(string? value, bool defaultValue = false) =>
            string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || defaultValue;
    }
}
