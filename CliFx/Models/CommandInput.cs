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
        /// Can be null if command was not specified.
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
            CommandName = commandName; // can be null
            Options = options.GuardNotNull(nameof(options));
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
                buffer.Append(CommandName);

            foreach (var option in Options)
            {
                buffer.AppendIfEmpty(' ');
                buffer.Append(option);
            }

            return buffer.ToString();
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