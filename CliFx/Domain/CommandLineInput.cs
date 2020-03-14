using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CliFx.Internal;

namespace CliFx.Domain
{
    internal partial class CommandLineInput
    {
        public IReadOnlyList<CommandDirectiveInput> Directives { get; }

        public IReadOnlyList<CommandUnboundArgumentInput> UnboundArguments { get; }

        public IReadOnlyList<CommandOptionInput> Options { get; }

        public bool IsDebugDirectiveSpecified => Directives.Any(d => d.IsDebugDirective);

        public bool IsPreviewDirectiveSpecified => Directives.Any(d => d.IsPreviewDirective);

        public bool IsHelpOptionSpecified => Options.Any(o => o.IsHelpOption);

        public bool IsVersionOptionSpecified => Options.Any(o => o.IsVersionOption);

        public CommandLineInput(
            IReadOnlyList<CommandDirectiveInput> directives,
            IReadOnlyList<CommandUnboundArgumentInput> unboundArguments,
            IReadOnlyList<CommandOptionInput> options)
        {
            Directives = directives;
            UnboundArguments = unboundArguments;
            Options = options;
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();

            foreach (var directive in Directives)
            {
                buffer.AppendIfNotEmpty(' ');
                buffer.Append(directive);
            }

            foreach (var argument in UnboundArguments)
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
            var builder = new CommandLineInputBuilder();

            var currentOptionAlias = "";
            var currentOptionValues = new List<string>();

            bool TryParseDirective(string argument)
            {
                if (!string.IsNullOrWhiteSpace(currentOptionAlias))
                    return false;

                if (!argument.StartsWith("[", StringComparison.OrdinalIgnoreCase) ||
                    !argument.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                    return false;

                var directive = argument.Substring(1, argument.Length - 2);
                builder.AddDirective(directive);

                return true;
            }

            bool TryParseArgument(string argument)
            {
                if (!string.IsNullOrWhiteSpace(currentOptionAlias))
                    return false;

                builder.AddUnboundArgument(argument);

                return true;
            }

            bool TryParseOptionName(string argument)
            {
                if (!argument.StartsWith("--", StringComparison.OrdinalIgnoreCase))
                    return false;

                if (!string.IsNullOrWhiteSpace(currentOptionAlias))
                    builder.AddOption(currentOptionAlias, currentOptionValues);

                currentOptionAlias = argument.Substring(2);
                currentOptionValues = new List<string>();

                return true;
            }

            bool TryParseOptionShortName(string argument)
            {
                if (!argument.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                    return false;

                foreach (var c in argument.Substring(1))
                {
                    if (!string.IsNullOrWhiteSpace(currentOptionAlias))
                        builder.AddOption(currentOptionAlias, currentOptionValues);

                    currentOptionAlias = c.AsString();
                    currentOptionValues = new List<string>();
                }

                return true;
            }

            bool TryParseOptionValue(string argument)
            {
                if (string.IsNullOrWhiteSpace(currentOptionAlias))
                    return false;

                currentOptionValues.Add(argument);

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

            if (!string.IsNullOrWhiteSpace(currentOptionAlias))
                builder.AddOption(currentOptionAlias, currentOptionValues);

            return builder.Build();
        }
    }

    internal partial class CommandLineInput
    {
        private static IReadOnlyList<CommandDirectiveInput> EmptyDirectives { get; } = new CommandDirectiveInput[0];

        private static IReadOnlyList<CommandUnboundArgumentInput> EmptyUnboundArguments { get; } = new CommandUnboundArgumentInput[0];

        private static IReadOnlyList<CommandOptionInput> EmptyOptions { get; } = new CommandOptionInput[0];

        public static CommandLineInput Empty { get; } = new CommandLineInput(EmptyDirectives, EmptyUnboundArguments, EmptyOptions);
    }
}