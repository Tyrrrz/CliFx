using System;
using CliFx.Infrastructure;
using CliFx.Parsing;

namespace CliFx.Help;

internal static class ConsoleExtensions
{
    extension(ConsoleWriter consoleWriter)
    {
        public void WriteException(Exception exception) =>
            new ExceptionWriter(consoleWriter).WriteException(exception);

        public void WriteHelp(HelpContext context) =>
            new HelpWriter(context, consoleWriter).WriteHelpText();

        public void WriteCommandLine(ParsedCommandLine commandLine) =>
            new ParsedCommandLineWriter(commandLine, consoleWriter).Write();
    }

    extension(IConsole console)
    {
        public void WriteException(Exception exception) => console.Error.WriteException(exception);

        public void WriteHelp(HelpContext context) => console.Output.WriteHelp(context);

        public void WriteCommandLine(ParsedCommandLine commandLine) =>
            console.Output.WriteCommandLine(commandLine);
    }
}
