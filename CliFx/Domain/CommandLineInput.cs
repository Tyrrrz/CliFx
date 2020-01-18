using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Internal;

namespace CliFx.Domain
{
    internal partial class CommandLineInput
    {
        public IReadOnlyList<string> Directives { get; }

        public IReadOnlyList<string> Arguments { get; }

        public IReadOnlyList<CommandOptionInput> Options { get; }

        public bool IsDebugDirectiveSpecified => Directives.Contains("debug", StringComparer.OrdinalIgnoreCase);

        public bool IsPreviewDirectiveSpecified => Directives.Contains("preview", StringComparer.OrdinalIgnoreCase);

        public bool IsHelpOptionSpecified => Options.Any(o =>
            string.Equals(o.Alias, "help", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(o.Alias, "h", StringComparison.OrdinalIgnoreCase));

        public bool IsVersionOptionSpecified => Options.Any(o =>
            string.Equals(o.Alias, "version", StringComparison.OrdinalIgnoreCase));

        public CommandLineInput(
            IReadOnlyList<string> directives,
            IReadOnlyList<string> arguments,
            IReadOnlyList<CommandOptionInput> options)
        {
            Directives = directives;
            Arguments = arguments;
            Options = options;
        }
    }

    internal partial class CommandLineInput
    {
        // TODO: refactor
        public static CommandLineInput Parse(IReadOnlyList<string> commandLineArguments)
        {
            var directives = new List<string>();
            var arguments = new List<string>();
            var optionsDic = new Dictionary<string, List<string>>();

            // Option aliases and values are parsed in pairs so we need to keep track of last alias
            var lastOptionAlias = "";

            foreach (var cmdArg in commandLineArguments)
            {
                // Encountered option name
                if (cmdArg.StartsWith("--", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract option alias
                    lastOptionAlias = cmdArg.Substring(2);

                    if (!optionsDic.ContainsKey(lastOptionAlias))
                        optionsDic[lastOptionAlias] = new List<string>();
                }

                // Encountered short option name or multiple short option names
                else if (cmdArg.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle stacked options
                    foreach (var c in cmdArg.Substring(1))
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
                    if (cmdArg.StartsWith("[", StringComparison.OrdinalIgnoreCase) &&
                        cmdArg.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract directive
                        var directive = cmdArg.Substring(1, cmdArg.Length - 2);

                        directives.Add(directive);
                    }
                    else
                    {
                        arguments.Add(cmdArg);
                    }
                }

                // Encountered option value
                else if (!string.IsNullOrWhiteSpace(lastOptionAlias))
                {
                    optionsDic[lastOptionAlias].Add(cmdArg);
                }
            }

            var options = optionsDic.Select(p => new CommandOptionInput(p.Key, p.Value)).ToArray();

            return new CommandLineInput(directives, arguments, options);
        }
    }
}