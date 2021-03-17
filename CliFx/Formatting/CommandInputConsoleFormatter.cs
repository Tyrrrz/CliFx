using System;
using CliFx.Infrastructure;
using CliFx.Input;
using CliFx.Utils.Extensions;

namespace CliFx.Formatting
{
    internal class CommandInputConsoleFormatter : ConsoleFormatter
    {
        private readonly CommandInput _commandInput;

        public CommandInputConsoleFormatter(ConsoleWriter consoleWriter, CommandInput commandInput)
            : base(consoleWriter)
        {
            _commandInput = commandInput;
        }

        public void WriteCommandInput()
        {
            // Command name
            if (!string.IsNullOrWhiteSpace(_commandInput.CommandName))
            {
                Write(ConsoleColor.Cyan, _commandInput.CommandName);
                Write(' ');
            }

            // Parameters
            foreach (var parameter in _commandInput.Parameters)
            {
                Write('<');
                Write(ConsoleColor.White, parameter.Value);
                Write('>');
                Write(' ');
            }

            // Options
            foreach (var option in _commandInput.Options)
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
            new CommandInputConsoleFormatter(console.Output, commandInput).WriteCommandInput();
    }
}