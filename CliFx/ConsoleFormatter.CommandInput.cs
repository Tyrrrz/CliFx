using System;
using System.Linq;
using CliFx.Input;

namespace CliFx
{
    internal partial class ConsoleFormatter
    {
        public void WriteCommandInput(CommandInput input)
        {
            // Command name
            if (!string.IsNullOrWhiteSpace(input.CommandName))
            {
                Write(ConsoleColor.Cyan, input.CommandName);
                Write(' ');
            }

            // Parameters
            foreach (var parameter in input.Parameters)
            {
                Write('<');
                Write(ConsoleColor.White, parameter.Value);
                Write('>');
                Write(' ');
            }

            // Options
            foreach (var option in input.Options)
            {
                Write('[');

                // Identifier
                Write(ConsoleColor.White, option.GetFormattedIdentifier());

                // Value(s)
                if (option.Values.Any())
                {
                    Write(' ');
                    Write(ConsoleColor.White, option.GetFormattedValues());
                }

                Write(']');
                Write(' ');
            }

            WriteLine();
        }
    }
}