using System.Collections.Generic;
using System.Text;
using CliFx.Internal;

namespace CliFx.Models
{
    /// <summary>
    /// Parsed command line input.
    /// </summary>
    public partial class CommandInput
    {
        /// <summary>
        /// Specified command name.
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        /// Specified options.
        /// </summary>
        public IReadOnlyList<CommandOptionInput> Options { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(string commandName, IReadOnlyList<CommandOptionInput> options)
        {
            CommandName = commandName;
            Options = options;
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(IReadOnlyList<CommandOptionInput> options)
            : this(null, options)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(string commandName)
            : this(commandName, new CommandOptionInput[0])
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput()
            : this(null, new CommandOptionInput[0])
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var buffer = new StringBuilder();

            if (!CommandName.IsNullOrWhiteSpace())
            {
                buffer.Append(CommandName);
                buffer.Append(' ');
            }

            foreach (var option in Options)
            {
                buffer.Append(option);
                buffer.Append(' ');
            }

            return buffer.Trim().ToString();
        }
    }

    public partial class CommandInput
    {
        /// <summary>
        /// Empty input.
        /// </summary>
        public static CommandInput Empty { get; } = new CommandInput();
    }
}