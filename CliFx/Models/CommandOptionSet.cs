using System.Collections.Generic;
using CliFx.Internal;

namespace CliFx.Models
{
    public partial class CommandOptionSet
    {
        public string CommandName { get; }

        public IReadOnlyDictionary<string, string> Options { get; }

        public CommandOptionSet(string commandName, IReadOnlyDictionary<string, string> options)
        {
            CommandName = commandName;
            Options = options;
        }

        public CommandOptionSet(IReadOnlyDictionary<string, string> options)
            : this(null, options)
        {
        }

        public CommandOptionSet(string commandName)
            : this(commandName, new Dictionary<string, string>())
        {
        }

        public override string ToString() => !CommandName.IsNullOrWhiteSpace()
            ? $"{CommandName} / {Options.Count} option(s)"
            : $"{Options.Count} option(s)";
    }

    public partial class CommandOptionSet
    {
        public static CommandOptionSet Empty { get; } = new CommandOptionSet(new Dictionary<string, string>());
    }
}