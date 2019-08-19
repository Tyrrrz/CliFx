using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            var commandNameBuilder = new StringBuilder();
            var optionsDic = new Dictionary<string, List<string>>();

            var lastOptionAlias = "";

            foreach (var commandLineArgument in commandLineArguments)
            {
                // Encountered option name
                if (commandLineArgument.StartsWith("--", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract option alias
                    lastOptionAlias = commandLineArgument.Substring(2);

                    if (!optionsDic.ContainsKey(lastOptionAlias))
                        optionsDic[lastOptionAlias] = new List<string>();
                }

                // Encountered short option name or multiple thereof
                else if (commandLineArgument.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle stacked options
                    foreach (var c in commandLineArgument.Substring(1))
                    {
                        // Extract option alias
                        lastOptionAlias = c.AsString();

                        if (!optionsDic.ContainsKey(lastOptionAlias))
                            optionsDic[lastOptionAlias] = new List<string>();
                    }
                }

                // Encountered command name or part thereof
                else if (lastOptionAlias.IsNullOrWhiteSpace())
                {
                    commandNameBuilder.AppendIfNotEmpty(' ');
                    commandNameBuilder.Append(commandLineArgument);
                }

                // Encountered option value
                else if (!lastOptionAlias.IsNullOrWhiteSpace())
                {
                    optionsDic[lastOptionAlias].Add(commandLineArgument);
                }
            }

            var commandName = commandNameBuilder.Length > 0 ? commandNameBuilder.ToString() : null;
            var options = optionsDic.Select(p => new CommandOptionInput(p.Key, p.Value)).ToArray();

            return new CommandInput(commandName, options);
        }
    }
}