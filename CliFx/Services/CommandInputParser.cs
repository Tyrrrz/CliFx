using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Default implementation of <see cref="ICommandInputParser"/>.
    /// </summary>
    public class CommandInputParser : ICommandInputParser
    {
        /// <inheritdoc />
        public CommandInput ParseCommandInput(IReadOnlyList<string> commandLineArguments)
        {
            commandLineArguments.GuardNotNull(nameof(commandLineArguments));

            // Initialize command name placeholder
            string commandName = null;

            // Initialize options
            var rawOptions = new Dictionary<string, List<string>>();

            // Keep track of the last option's name
            string optionName = null;

            // Loop through all arguments
            var encounteredFirstOption = false;
            foreach (var commandLineArgument in commandLineArguments)
            {
                // Option name
                if (commandLineArgument.StartsWith("--", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract option name (skip 2 chars)
                    optionName = commandLineArgument.Substring(2);

                    if (rawOptions.GetValueOrDefault(optionName) == null)
                        rawOptions[optionName] = new List<string>();

                    encounteredFirstOption = true;
                }

                // Short option name
                else if (commandLineArgument.StartsWith("-", StringComparison.OrdinalIgnoreCase) && commandLineArgument.Length == 2)
                {
                    // Extract option name (skip 1 char)
                    optionName = commandLineArgument.Substring(1);

                    if (rawOptions.GetValueOrDefault(optionName) == null)
                        rawOptions[optionName] = new List<string>();

                    encounteredFirstOption = true;
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

                    encounteredFirstOption = true;
                }

                // Command name
                else if (!encounteredFirstOption)
                {
                    if (commandName.IsNullOrWhiteSpace())
                        commandName = commandLineArgument;
                    else
                        commandName += " " + commandLineArgument;
                }

                // Option value
                else if (!optionName.IsNullOrWhiteSpace())
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    rawOptions[optionName].Add(commandLineArgument);
                }
            }

            return new CommandInput(commandName, rawOptions.Select(p => new CommandOptionInput(p.Key, p.Value)).ToArray());
        }
    }
}