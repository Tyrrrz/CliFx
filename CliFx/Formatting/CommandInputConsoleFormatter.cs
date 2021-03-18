using System;
using CliFx.Infrastructure;
using CliFx.Input;
using CliFx.Utils.Extensions;

namespace CliFx.Formatting
{
    internal class CommandInputConsoleFormatter : ConsoleFormatter
    {
        public CommandInputConsoleFormatter(ConsoleWriter consoleWriter)
            : base(consoleWriter)
        {
        }

        public void WriteCommandInput(CommandInput commandInput)
        {
            // Command name
            if (!string.IsNullOrWhiteSpace(commandInput.CommandName))
            {
                Write(ConsoleColor.Cyan, commandInput.CommandName);
                Write(' ');
            }

            // Parameters
            foreach (var parameter in commandInput.Parameters)
            {
                Write('<');
                Write(ConsoleColor.White, parameter.Value);
                Write('>');
                Write(' ');
            }

            // Options
            foreach (var option in commandInput.Options)
            {
                Write('[');

                // Identifier
                Write(ConsoleColor.White, option.GetFormattedIdentifier());

                // Value(s)
                foreach (var value in option.Values)
                {
                    Write(' ');
                    Write(value.Quote());
                }

                Write(']');
                Write(' ');
            }

            WriteLine();
        }
    }

    internal static class CommandInputConsoleFormatterExtensions
    {
        public static void WriteCommandInput(this IConsole console, CommandInput commandInput) =>
            new CommandInputConsoleFormatter(console.Output).WriteCommandInput(commandInput);
    }
}