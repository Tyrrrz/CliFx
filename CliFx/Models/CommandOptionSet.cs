using System.Collections.Generic;
using System.Linq;
using CliFx.Internal;

namespace CliFx.Models
{
    public partial class CommandOptionSet
    {
        public string CommandName { get; }

        public IReadOnlyList<CommandOption> Options { get; }

        public CommandOptionSet(string commandName, IReadOnlyList<CommandOption> options)
        {
            CommandName = commandName;
            Options = options;
        }

        public CommandOptionSet(IReadOnlyList<CommandOption> options)
            : this(null, options)
        {
        }

        public CommandOptionSet(string commandName)
            : this(commandName, new CommandOption[0])
        {
        }

        public override string ToString()
        {
            if (Options.Any())
            {
                var optionsJoined = Options.Select(o => o.Name).JoinToString(", ");
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
        public static CommandOptionSet Empty { get; } = new CommandOptionSet(new CommandOption[0]);
    }
}