using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public bool IsHelpOptionSpecified =>
            Options.Any(o => CommandOptionSchema.HelpOption.MatchesNameOrShortName(o.Alias));

        public bool IsVersionOptionSpecified =>
            Options.Any(o => CommandOptionSchema.VersionOption.MatchesNameOrShortName(o.Alias));

        public CommandLineInput(
            IReadOnlyList<string> directives,
            IReadOnlyList<string> arguments,
            IReadOnlyList<CommandOptionInput> options)
        {
            Directives = directives;
            Arguments = arguments;
            Options = options;
        }

        public CommandLineInput(
            IReadOnlyList<string> arguments,
            IReadOnlyList<CommandOptionInput> options)
            : this(new string[0], arguments, options)
        {
        }

        public CommandLineInput(IReadOnlyList<string> arguments)
            : this(arguments, new CommandOptionInput[0])
        {
        }

        public CommandLineInput(IReadOnlyList<CommandOptionInput> options)
            : this(new string[0], options)
        {
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();

            foreach (var directive in Directives)
            {
                buffer.AppendIfNotEmpty(' ');
                buffer
                    .Append('[')
                    .Append(directive)
                    .Append(']');
            }

            foreach (var argument in Arguments)
            {
                buffer.AppendIfNotEmpty(' ');
                buffer.Append(argument);
            }

            foreach (var option in Options)
            {
                buffer.AppendIfNotEmpty(' ');
                buffer.Append(option);
            }

            return buffer.ToString();
        }
    }

    internal partial class CommandLineInput
    {
        public static CommandLineInput Parse(IReadOnlyList<string> commandLineArguments)
        {
            var directives = new List<string>();
            var arguments = new List<string>();
            var optionsDic = new Dictionary<string, List<string>>();

            // Option aliases and values are parsed in pairs so we need to keep track of last alias
            var lastOptionAlias = "";

            bool TryParseDirective(string argument)
            {
                if (!string.IsNullOrWhiteSpace(lastOptionAlias))
                    return false;

                if (!argument.StartsWith("[", StringComparison.OrdinalIgnoreCase) ||
                    !argument.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                    return false;

                var directive = argument.Substring(1, argument.Length - 2);
                directives.Add(directive);

                return true;
            }

            bool TryParseArgument(string argument)
            {
                if (!string.IsNullOrWhiteSpace(lastOptionAlias))
                    return false;

                arguments.Add(argument);

                return true;
            }

            bool TryParseOptionName(string argument)
            {
                if (!argument.StartsWith("--", StringComparison.OrdinalIgnoreCase))
                    return false;

                lastOptionAlias = argument.Substring(2);

                if (!optionsDic.ContainsKey(lastOptionAlias))
                    optionsDic[lastOptionAlias] = new List<string>();

                return true;
            }

            bool TryParseOptionShortName(string argument)
            {
                if (!argument.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                    return false;

                foreach (var c in argument.Substring(1))
                {
                    lastOptionAlias = c.AsString();

                    if (!optionsDic.ContainsKey(lastOptionAlias))
                        optionsDic[lastOptionAlias] = new List<string>();
                }

                return true;
            }

            bool TryParseOptionValue(string argument)
            {
                if (string.IsNullOrWhiteSpace(lastOptionAlias))
                    return false;

                optionsDic[lastOptionAlias].Add(argument);

                return true;
            }

            foreach (var argument in commandLineArguments)
            {
                var _ =
                    TryParseOptionName(argument) ||
                    TryParseOptionShortName(argument) ||
                    TryParseDirective(argument) ||
                    TryParseArgument(argument) ||
                    TryParseOptionValue(argument);
            }

            var options = optionsDic.Select(p => new CommandOptionInput(p.Key, p.Value)).ToArray();

            return new CommandLineInput(directives, arguments, options);
        }
    }

    internal partial class CommandLineInput
    {
        public static CommandLineInput Empty { get; } =
            new CommandLineInput(new string[0], new string[0], new CommandOptionInput[0]);
    }
}