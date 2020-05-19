using System.Collections.Generic;

namespace CliFx.Domain
{
    internal class CommandLineInputBuilder
    {
        private readonly List<string> _directives = new List<string>();
        private readonly List<string> _parameters = new List<string>();
        private readonly Dictionary<string, IReadOnlyList<string>> _options = new Dictionary<string, IReadOnlyList<string>>();

        private string? _commandName;

        public CommandLineInputBuilder SetCommandName(string name)
        {
            _commandName = name;
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

        public CommandLineInputBuilder AddOption(string alias, IReadOnlyList<string> values)
        {
            _options[alias] = values;
            return this;
        }

        public CommandLineInputBuilder AddOption(string alias, params string[] values) =>
            AddOption(alias, (IReadOnlyList<string>) values);

        public CommandLineInput Build() => new CommandLineInput(_directives, _commandName, _parameters, _options);
    }
}