using System.Collections.Generic;
using System.Linq;
using System.Text;
using CliFx.Internal;

namespace CliFx.Models
{
    public partial class CommandInput
    {
        public string CommandName { get; }

        public IReadOnlyList<CommandOptionInput> Options { get; }

        public CommandInput(string commandName, IReadOnlyList<CommandOptionInput> options)
        {
            CommandName = commandName;
            Options = options;
        }

        public CommandInput(IReadOnlyList<CommandOptionInput> options)
            : this(null, options)
        {
        }

        public CommandInput(string commandName)
            : this(commandName, new CommandOptionInput[0])
        {
        }

        public CommandInput()
            : this(null, new CommandOptionInput[0])
        {
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();

            if (!CommandName.IsNullOrWhiteSpace())
            {
                buffer.Append(CommandName);
            }

            if (Options.Any())
            {
                if (buffer.Length > 0)
                    buffer.Append(' ');

                buffer.Append('[');

                foreach (var option in Options)
                {
                    buffer.Append(option.Name);
                }

                buffer.Append(']');
            }

            return buffer.ToString();
        }
    }

    public partial class CommandInput
    {
        public static CommandInput Empty { get; } = new CommandInput();
    }
}