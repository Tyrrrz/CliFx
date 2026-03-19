using CliFx.Infrastructure;
using CliFx.Parsing;

namespace CliFx.Formatting;

internal static class ParsedCommandLineConsoleFormatterExtensions
{
    extension(ConsoleWriter consoleWriter)
    {
        public void WriteCommandLine(ParsedCommandLine commandLine) =>
            new ParsedCommandLineConsoleFormatter(consoleWriter, commandLine).Write();
    }

    extension(IConsole console)
    {
        public void WriteCommandLine(ParsedCommandLine commandLine) =>
            console.Output.WriteCommandLine(commandLine);
    }
}
