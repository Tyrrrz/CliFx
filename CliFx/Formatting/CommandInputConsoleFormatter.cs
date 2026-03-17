using System;
using CliFx.Infrastructure;
using CliFx.Parsing;

namespace CliFx.Formatting;

internal class CommandInputConsoleFormatter(ConsoleWriter consoleWriter)
    : ConsoleFormatter(consoleWriter)
{
    private void WriteCommandLineArguments(ParsedCommandLine commandLine)
    {
        Write("Command-line:");
        WriteLine();

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

            Write(ConsoleColor.White, parsedOption.GetFormattedIdentifier());

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

    private void WriteEnvironmentVariables(ParsedCommandLine commandLine)
    {
        Write("Environment:");
        WriteLine();

        foreach (var parsedEnvironmentVariable in commandLine.EnvironmentVariables)
        {
            WriteHorizontalMargin();

            Write(ConsoleColor.White, parsedEnvironmentVariable.Name);
            Write('=');
            Write('"');
            Write(parsedEnvironmentVariable.Value);
            Write('"');

            WriteLine();
        }
    }

    public void WriteCommandInput(ParsedCommandLine commandLine)
    {
        WriteCommandLineArguments(commandLine);
        WriteLine();
        WriteEnvironmentVariables(commandLine);
    }
}

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
