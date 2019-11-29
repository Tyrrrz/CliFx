﻿using System;
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
        private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;

        /// <summary>
        /// Initializes an instance of <see cref="CommandInputParser"/>
        /// </summary>
        public CommandInputParser(IEnvironmentVariablesProvider environmentVariablesProvider)
        {
            _environmentVariablesProvider = environmentVariablesProvider;
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInputParser"/>
        /// </summary>
        public CommandInputParser()
            : this(new EnvironmentVariablesProvider())
        {
        }

        /// <inheritdoc />
        public CommandInput ParseCommandInput(IReadOnlyList<string> commandLineArguments)
        {
            var arguments = new List<string>();
            var directives = new List<string>();
            var optionsDic = new Dictionary<string, List<string>>();

            // Option aliases and values are parsed in pairs so we need to keep track of last alias
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

                // Encountered short option name or multiple short option names
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

                // Encountered directive or (part of) command name
                else if (string.IsNullOrWhiteSpace(lastOptionAlias))
                {
                    if (commandLineArgument.StartsWith("[", StringComparison.OrdinalIgnoreCase) &&
                        commandLineArgument.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract directive
                        var directive = commandLineArgument.Substring(1, commandLineArgument.Length - 2);

                        directives.Add(directive);
                    }
                    else
                    {
                        arguments.Add(commandLineArgument);
                    }
                }

                // Encountered option value
                else if (!string.IsNullOrWhiteSpace(lastOptionAlias))
                {
                    optionsDic[lastOptionAlias].Add(commandLineArgument);
                }
            }

            var options = optionsDic.Select(p => new CommandOptionInput(p.Key, p.Value)).ToArray();

            var environmentVariables = _environmentVariablesProvider.GetEnvironmentVariables();

            return new CommandInput(arguments, directives, options, environmentVariables);
        }
    }
}