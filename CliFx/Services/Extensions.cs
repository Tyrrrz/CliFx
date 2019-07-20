using CliFx.Models;

namespace CliFx.Services
{
    public static class Extensions
    {
        public static void Write(this IConsoleWriter consoleWriter, string text) => consoleWriter.Write(new TextSpan(text));

        public static void WriteLine(this IConsoleWriter consoleWriter, string text) => consoleWriter.WriteLine(new TextSpan(text));
    }
}