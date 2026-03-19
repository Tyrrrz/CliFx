using System;
using CliFx.Infrastructure;
using CliFx.Parsing;

namespace CliFx.Formatting;

internal class ParseCommandLineConsoleFormatter(
    ConsoleWriter consoleWriter,
    ParsedCommandLine commandLine
) : ConsoleFormatter(consoleWriter)
{
    public void Write()
    {
        WriteHorizontalMargin();

        // Command name
        if (!string.IsNullOrWhiteSpace(commandLine.CommandName))
        {
            Write(ConsoleColor.Cyan, commandLine.CommandName);
            Write(' ');
        }

        // Positional arguments
        foreach (var positionalArgument in commandLine.PositionalArguments)
        {
            Write('<');
            Write(ConsoleColor.White, positionalArgument.Value);
            Write('>');
            Write(' ');
        }

        // Options
        foreach (var parsedOption in commandLine.Options)
        {
            Write('[');

            Write(ConsoleColor.White, parsedOption.ToString());

            foreach (var value in parsedOption.Values)
            {
                Write(' ');
                Write('"');
                Write(value);
                Write('"');
            }

            Write(']');
            Write(' ');
        }

        WriteLine();
    }
}
