using System;
using System.Linq;
using CliFx.Infrastructure;
using CliFx.Input;

namespace CliFx.Formatting
{
    internal partial class ConsoleFormatter
    {
        public static void WriteCommandInput(IConsole console, CommandInput input)
        {
            var formatter = new ConsoleFormatter(console, false);

            // Command name
            if (!string.IsNullOrWhiteSpace(input.CommandName))
            {
                formatter.Write(ConsoleColor.Cyan, input.CommandName);
                formatter.Write(' ');
            }

            // Parameters
            foreach (var parameter in input.Parameters)
            {
                formatter.Write('<');
                formatter.Write(ConsoleColor.White, parameter.Value);
                formatter.Write('>');
                formatter.Write(' ');
            }

            // Options
            foreach (var option in input.Options)
            {
                formatter.Write('[');

                // Identifier
                formatter.Write(ConsoleColor.White, option.GetFormattedIdentifier());

                // Value(s)
                if (option.Values.Any())
                {
                    formatter.Write(' ');
                    formatter.Write(ConsoleColor.White, option.GetFormattedValues());
                }

                formatter.Write(']');
                formatter.Write(' ');
            }

            formatter.WriteLine();
        }
    }
}