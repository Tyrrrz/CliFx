namespace CliFx.Utilities
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Text utilities.
    /// </summary>
    public static class TextUtils
    {
        private readonly static Regex _newLinesRegex = new Regex(@"\n\r", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Replaces new line characters to match 'Environment.NewLine'.
        /// </summary>
        public static string AdjustNewLines(string text)
        {
            return _newLinesRegex.Replace(text, Environment.NewLine);
        }

        /// <summary>
        /// Converts tabs to spaces.
        /// </summary>
        public static string ConvertTabsToSpaces(string text, int width = 2)
        {
            return text.Replace("\t", new string(' ', width));
        }
    }
}