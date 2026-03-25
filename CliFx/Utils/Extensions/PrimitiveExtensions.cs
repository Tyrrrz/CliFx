using System;

namespace CliFx.Utils.Extensions;

internal static class PrimitiveExtensions
{
    extension(bool)
    {
        public static bool ParseOrDefault(string? value, bool defaultValue = false)
        {
            if (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(value, bool.FalseString, StringComparison.OrdinalIgnoreCase))
                return false;

            return defaultValue;
        }
    }
}
