using System.Collections.Generic;
using CliFx.Domain;

namespace CliFx.Tests.Internal
{
    internal class CommandLineInputBuilder
    {
        private readonly List<CommandDirectiveInput> _directives = new List<CommandDirectiveInput>();
        private readonly List<CommandParameterInput> _parameters = new List<CommandParameterInput>();
        private readonly List<CommandOptionInput> _options = new List<CommandOptionInput>();

        private string? _commandName;

        public CommandLineInputBuilder SetCommandName(string commandName)
        {
            _commandName = commandName;
            return this;
        }

        public CommandLineInputBuilder AddDirective(string directive)
        {
            _directives.Add(new CommandDirectiveInput(directive));
            return this;
        }

        public CommandLineInputBuilder AddParameter(string parameter)
        {
            _parameters.Add(new CommandParameterInput(parameter));
            return this;
        }

        public CommandLineInputBuilder AddOption(string alias, params string[] values)
        {
            _options.Add(new CommandOptionInput(alias, values));
            return this;
        }

        public CommandLineInput Build() => new CommandLineInput(
            _directives,
            _commandName,
            _parameters,
            _options
        );
    }
}