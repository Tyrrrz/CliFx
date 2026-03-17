using CliFx.Infrastructure;
using CliFx.Parsing;

namespace CliFx.Formatting;

internal static class CommandInputConsoleFormatterExtensions
{
    public static void WriteCommandInput(
        this ConsoleWriter consoleWriter,
        ParsedCommandLine commandLine
    ) => new CommandInputConsoleFormatter(consoleWriter).WriteCommandInput(commandLine);

    extension(IConsole console)
    {
        public void WriteCommandInput(ParsedCommandLine commandLine) =>
            console.Output.WriteCommandInput(commandLine);
    }
}
