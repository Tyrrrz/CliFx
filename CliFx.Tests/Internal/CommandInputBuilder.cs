namespace CliFx.Tests.Internal
{
    using System.Collections.Generic;
    using CliFx.Input;

    internal class CommandInputBuilder
    {
        private readonly List<DirectiveInput> _directives = new List<DirectiveInput>();
        private readonly List<CommandParameterInput> _parameters = new List<CommandParameterInput>();
        private readonly List<CommandOptionInput> _options = new List<CommandOptionInput>();

        private string? _commandName;

        public CommandInputBuilder SetCommandName(string commandName)
        {
            _commandName = commandName;
            return this;
        }

        public CommandInputBuilder AddDirective(string directive)
        {
            _directives.Add(new DirectiveInput(directive));
            return this;
        }

        public CommandInputBuilder AddParameter(string parameter)
        {
            _parameters.Add(new CommandParameterInput(parameter));
            return this;
        }

        public CommandInputBuilder AddOption(string alias, params string[] values)
        {
            _options.Add(new CommandOptionInput(alias, values));
            return this;
        }

        public CommandInput Build()
        {
            return new CommandInput(
                false,
                _directives,
                _commandName,
                _parameters,
                _options
                );
        }
    }
}