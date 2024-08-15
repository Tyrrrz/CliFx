using System;
using CliFx.Infrastructure;
using CliFx.Parsing;

namespace CliFx.Formatting;

internal class CommandArgumentsConsoleFormatter(ConsoleWriter consoleWriter)
    : ConsoleFormatter(consoleWriter)
{
    public void WriteCommandArguments(CommandArguments commandArguments)
    {
        // Command name
        if (!string.IsNullOrWhiteSpace(commandArguments.CommandName))
        {
            Write(ConsoleColor.Cyan, commandArguments.CommandName);
            Write(' ');
        }

        // Parameters
        foreach (var parameterInput in commandArguments.Parameters)
        {
            Write('<');
            Write(ConsoleColor.White, parameterInput.Value);
            Write('>');
            Write(' ');
        }

        // Options
        foreach (var optionInput in commandArguments.Options)
        {
            Write('[');

            // Identifier
            Write(ConsoleColor.White, optionInput.FormattedIdentifier);

            // Value(s)
            foreach (var value in optionInput.Values)
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

internal static class CommandInputConsoleFormatterExtensions
{
    public static void WriteCommandInput(
        this ConsoleWriter consoleWriter,
        CommandArguments commandArguments
    ) =>
        new CommandArgumentsConsoleFormatter(consoleWriter).WriteCommandArguments(commandArguments);

    public static void WriteCommandInput(
        this IConsole console,
        CommandArguments commandArguments
    ) => console.Output.WriteCommandInput(commandArguments);
}
