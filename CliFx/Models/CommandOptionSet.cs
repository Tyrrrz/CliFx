using System.Collections.Generic;
using System.Linq;
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

        public override string ToString()
        {
            if (Options.Any())
            {
                var optionsJoined = Options.Select(o => o.Key).JoinToString(", ");
                return !CommandName.IsNullOrWhiteSpace() ? $"{CommandName} / [{optionsJoined}]" : $"[{optionsJoined}]";
            }
            else
            {
                return !CommandName.IsNullOrWhiteSpace() ? $"{CommandName} / no options" : "no options";
            }
        }
    }

    public partial class CommandOptionSet
    {
        public static CommandOptionSet Empty { get; } = new CommandOptionSet(new Dictionary<string, string>());
    }
}