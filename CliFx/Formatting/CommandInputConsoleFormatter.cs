﻿using System;
using System.Linq;
using CliFx.Infrastructure;
using CliFx.Input;

namespace CliFx.Formatting
{
    internal class CommandInputConsoleFormatter : ConsoleFormatter
    {
        public CommandInputConsoleFormatter(ConsoleWriter consoleWriter)
            : base(consoleWriter)
        {
        }

        private void WriteCommandLineArguments(CommandInput commandInput)
        {
            Write("Command line:");
            WriteLine();

            WriteHorizontalMargin();

            // Command name
            if (!string.IsNullOrWhiteSpace(commandInput.CommandName))
            {
                Write(ConsoleColor.Cyan, commandInput.CommandName);
                Write(' ');
            }

            // Parameters
            foreach (var parameterInput in commandInput.Parameters)
            {
                Write('<');
                Write(ConsoleColor.White, parameterInput.Value);
                Write('>');
                Write(' ');
            }

            // Options
            foreach (var optionInput in commandInput.Options)
            {
                Write('[');

                // Identifier
                Write(ConsoleColor.White, optionInput.GetFormattedIdentifier());

                // Value(s)
                foreach (var value in optionInput.Values)
                {
                    Write(' ');
                    Write(ConsoleColor.DarkGray, '"');
                    Write(value);
                    Write(ConsoleColor.DarkGray, '"');
                }

                Write(']');
                Write(' ');
            }

            WriteLine();
        }

        private void WriteEnvironmentVariables(CommandInput commandInput)
        {
            Write("Environment:");
            WriteLine();

            // Environment variables
            foreach (var environmentVariableInput in commandInput.EnvironmentVariables)
            {
                WriteHorizontalMargin();

                // Name
                Write(ConsoleColor.White, environmentVariableInput.Name);

                Write('=');

                // Value
                Write(ConsoleColor.DarkGray, '"');
                Write(environmentVariableInput.Value);
                Write(ConsoleColor.DarkGray, '"');

                WriteLine();
            }
        }

        public void WriteCommandInput(CommandInput commandInput)
        {
            WriteCommandLineArguments(commandInput);
            WriteLine();
            WriteEnvironmentVariables(commandInput);
        }
    }

    internal static class CommandInputConsoleFormatterExtensions
    {
        public static void WriteCommandInput(this IConsole console, CommandInput commandInput) =>
            new CommandInputConsoleFormatter(console.Output).WriteCommandInput(commandInput);
    }
}