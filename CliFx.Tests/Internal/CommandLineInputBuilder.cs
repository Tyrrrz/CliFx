using System.Collections.Generic;
using System.Linq;
using CliFx.Domain;

namespace CliFx.Tests.Internal
{
    internal class CommandLineInputBuilder
    {
        private readonly List<string> _directives = new List<string>();
        private readonly List<string> _parameters = new List<string>();
        private readonly List<KeyValuePair<string, IReadOnlyList<string>>> _options = new List<KeyValuePair<string, IReadOnlyList<string>>>();

        private string? _commandName;

        public CommandLineInputBuilder SetCommandName(string commandName)
        {
            _commandName = commandName;
            return this;
        }

        public CommandLineInputBuilder AddDirective(string directive)
        {
            _directives.Add(directive);
            return this;
        }

        public CommandLineInputBuilder AddParameter(string parameter)
        {
            _parameters.Add(parameter);
            return this;
        }

        public CommandLineInputBuilder AddOption(string alias, params string[] values)
        {
            _options.Add(KeyValuePair.Create<string, IReadOnlyList<string>>(alias, values));
            return this;
        }

        public CommandLineInput Build() => new CommandLineInput(
            _directives,
            _commandName,
            _parameters,
            _options.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        );
    }
}