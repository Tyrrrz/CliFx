using System.Collections.Generic;

namespace CliFx.Domain
{
    internal class CommandLineInputBuilder
    {
        private readonly List<CommandDirectiveInput> _directives = new List<CommandDirectiveInput>();
        private readonly List<CommandUnboundArgumentInput> _unboundArguments = new List<CommandUnboundArgumentInput>();
        private readonly List<CommandOptionInput> _options = new List<CommandOptionInput>();

        public CommandLineInputBuilder AddDirective(CommandDirectiveInput directive)
        {
            _directives.Add(directive);
            return this;
        }

        public CommandLineInputBuilder AddDirective(string directive) =>
            AddDirective(new CommandDirectiveInput(directive));

        public CommandLineInputBuilder AddUnboundArgument(CommandUnboundArgumentInput unboundArgument)
        {
            _unboundArguments.Add(unboundArgument);
            return this;
        }

        public CommandLineInputBuilder AddUnboundArgument(string unboundArgument) =>
            AddUnboundArgument(new CommandUnboundArgumentInput(unboundArgument));

        public CommandLineInputBuilder AddOption(CommandOptionInput option)
        {
            _options.Add(option);
            return this;
        }

        public CommandLineInputBuilder AddOption(string optionAlias, IReadOnlyList<string> values) =>
            AddOption(new CommandOptionInput(optionAlias, values));

        public CommandLineInputBuilder AddOption(string optionAlias, params string[] values) =>
            AddOption(optionAlias, (IReadOnlyList<string>) values);

        public CommandLineInput Build() => new CommandLineInput(_directives, _unboundArguments, _options);
    }
}