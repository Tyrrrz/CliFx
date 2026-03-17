using System;
using CliFx.Infrastructure;

namespace CliFx.Formatting;

internal static class ExceptionConsoleFormatterExtensions
{
    extension(ConsoleWriter consoleWriter)
    {
        public void WriteException(Exception exception) =>
            new ExceptionConsoleFormatter(consoleWriter).WriteException(exception);
    }

    extension(IConsole console)
    {
        public void WriteException(Exception exception) => console.Error.WriteException(exception);
    }
}
