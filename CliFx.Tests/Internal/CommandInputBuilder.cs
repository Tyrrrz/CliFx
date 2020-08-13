using System.Collections.Generic;
using CliFx.Input;

namespace CliFx.Tests.Internal
{
    internal class CommandInputBuilder
    {
        private readonly List<CommandDirectiveInput> _directives = new List<CommandDirectiveInput>();
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
            _directives.Add(new CommandDirectiveInput(directive));
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