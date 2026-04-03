using System;
using CliFx.Infrastructure;
using CliFx.Parsing;

namespace CliFx.Help;

internal class ParsedCommandLineWriter(ParsedCommandLine commandLine, ConsoleWriter consoleWriter)
    : FormattedConsoleWriter(consoleWriter)
{
    public void Write()
    {
        Write(ConsoleColor.DarkGray, "> ");

        // Command name
        if (!string.IsNullOrWhiteSpace(commandLine.CommandName))
        {
            Write('|');
            Write(ConsoleColor.Cyan, commandLine.CommandName);
            Write('|');
            Write(' ');
        }

        // Positional arguments
        foreach (var positionalArgument in commandLine.PositionalArguments)
        {
            Write('|');
            Write(ConsoleColor.DarkCyan, positionalArgument.ToString());
            Write('|');
            Write(' ');
        }

        // Options
        foreach (var parsedOption in commandLine.Options)
        {
            Write('|');
            Write(ConsoleColor.Yellow, parsedOption.ToString());
            Write('|');
            Write(' ');
        }

        WriteLine();
    }
}
