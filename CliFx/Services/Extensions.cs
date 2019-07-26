using System;
using System.Globalization;
using CliFx.Models;

namespace CliFx.Services
{
    public static class Extensions
    {
        public static void Write(this IConsoleWriter consoleWriter, string text) =>
            consoleWriter.Write(new TextSpan(text));

        public static void Write(this IConsoleWriter consoleWriter, IFormattable formattable) =>
            consoleWriter.Write(formattable.ToString(null, CultureInfo.InvariantCulture));

        public static void Write(this IConsoleWriter consoleWriter, object obj)
        {
            if (obj is IFormattable formattable)
                consoleWriter.Write(formattable);
            else
                consoleWriter.Write(obj.ToString());
        }

        public static void WriteLine(this IConsoleWriter consoleWriter, string text) =>
            consoleWriter.WriteLine(new TextSpan(text));

        public static void WriteLine(this IConsoleWriter consoleWriter, IFormattable formattable) =>
            consoleWriter.WriteLine(formattable.ToString(null, CultureInfo.InvariantCulture));

        public static void WriteLine(this IConsoleWriter consoleWriter, object obj)
        {
            if (obj is IFormattable formattable)
                consoleWriter.WriteLine(formattable);
            else
                consoleWriter.WriteLine(obj.ToString());
        }
    }
}