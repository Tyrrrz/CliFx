﻿using System;

namespace CliFx.Analyzers.Utils.Extensions
{
    internal static class StringExtensions
    {
        public static string TrimEnd(
            this string str,
            string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            while (str.EndsWith(sub, comparison))
                str = str.Substring(0, str.Length - sub.Length);

            return str;
        }
    }
}