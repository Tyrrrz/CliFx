using System;
using System.Collections.Generic;
using System.Globalization;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    public class CommandOptionParser : ICommandOptionParser
    {
        public CommandOptionSet ParseOptions(IReadOnlyList<string> commandLineArguments)
        {
            // Initialize command name placeholder
            string commandName = null;

            // Initialize options
            var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Keep track of the last option's name
            string optionName = null;

            // Loop through all arguments
            var isFirstArgument = true;
            foreach (var commandLineArgument in commandLineArguments)
            {
                // Option name
                if (commandLineArgument.StartsWith("--", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract option name (skip 2 chars)
                    optionName = commandLineArgument.Substring(2);
                    options[optionName] = null;
                }

                // Short option name
                else if (commandLineArgument.StartsWith("-", StringComparison.OrdinalIgnoreCase) && commandLineArgument.Length == 2)
                {
                    // Extract option name (skip 1 char)
                    optionName = commandLineArgument.Substring(1);
                    options[optionName] = null;
                }

                // Multiple stacked short options
                else if (commandLineArgument.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                {
                    optionName = null;
                    foreach (var c in commandLineArgument.Substring(1))
                    {
                        options[c.ToString(CultureInfo.InvariantCulture)] = null;
                    }
                }

                // Command name
                else if (isFirstArgument)
                {
                    commandName = commandLineArgument;
                }

                // Option value
                else if (!optionName.IsNullOrWhiteSpace())
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    options[optionName] = commandLineArgument;
                }

                isFirstArgument = false;
            }

            return new CommandOptionSet(commandName, options);
        }
    }
}