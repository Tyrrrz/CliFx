using System;
using System.Collections.Generic;
using System.Linq;
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
            var rawOptions = new Dictionary<string, List<string>>();

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

                    if (rawOptions.GetValueOrDefault(optionName) == null)
                        rawOptions[optionName] = new List<string>();
                }

                // Short option name
                else if (commandLineArgument.StartsWith("-", StringComparison.OrdinalIgnoreCase) && commandLineArgument.Length == 2)
                {
                    // Extract option name (skip 1 char)
                    optionName = commandLineArgument.Substring(1);

                    if (rawOptions.GetValueOrDefault(optionName) == null)
                        rawOptions[optionName] = new List<string>();
                }

                // Multiple stacked short options
                else if (commandLineArgument.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var c in commandLineArgument.Substring(1))
                    {
                        optionName = c.AsString();

                        if (rawOptions.GetValueOrDefault(optionName) == null)
                            rawOptions[optionName] = new List<string>();
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
                    rawOptions[optionName].Add(commandLineArgument);
                }

                isFirstArgument = false;
            }

            return new CommandOptionSet(commandName, rawOptions.Select(p => new CommandOption(p.Key, p.Value)).ToArray());
        }
    }
}