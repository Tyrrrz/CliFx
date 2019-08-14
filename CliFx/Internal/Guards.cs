using System;

namespace CliFx.Internal
{
    internal static class Guards
    {
        public static T GuardNotNull<T>(this T o, string argName = null) where T : class =>
            o ?? throw new ArgumentNullException(argName);

        public static int GuardNotZero(this int i, string argName = null) =>
            i != 0 ? i : throw new ArgumentException("Cannot be zero.", argName);
    }
}