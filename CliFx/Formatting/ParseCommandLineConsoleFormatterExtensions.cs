using CliFx.Infrastructure;
using CliFx.Parsing;

namespace CliFx.Formatting;

internal static class ParseCommandLineConsoleFormatterExtensions
{
    extension(ConsoleWriter consoleWriter)
    {
        public void WriteCommandLine(ParsedCommandLine commandLine) =>
            new ParseCommandLineConsoleFormatter(consoleWriter, commandLine).Write();
    }

    extension(IConsole console)
    {
        public void WriteCommandLine(ParsedCommandLine commandLine) =>
            console.Output.WriteCommandLine(commandLine);
    }
}
