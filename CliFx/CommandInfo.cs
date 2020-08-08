using System.Collections.Generic;
using CliFx.Domain;

namespace CliFx
{
    /// <summary>
    /// Stores command info inside <see cref="CliContext"/>.
    /// </summary>
    public sealed class CommandInfo
    {
        /// <summary>
        /// Command name.
        /// If the name is not set, the command is treated as a default command, i.e. the one that gets executed when the user
        /// does not specify a command name in the arguments.
        /// All commands in an application must have different names. Likewise, only one command without a name is allowed.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Whether command is default.
        /// </summary>
        public bool IsDefault => string.IsNullOrWhiteSpace(Name);

        /// <summary>
        /// Command description, which is used in help text.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Command manual text, which is used in help text.
        /// </summary>
        public string? Manual { get; }

        /// <summary>
        /// Whether command can run only in interactive mode.
        /// </summary>
        public bool InteractiveModeOnly { get; }

        /// <summary>
        /// Raw command line arguments.
        /// </summary>
        public IReadOnlyList<string> CommandLineArguments { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CliApplication"/>.
        /// </summary>
        internal CommandInfo(CommandSchema command, IReadOnlyList<string> commandLineArguments)
        {
            Name = command.Name;
            Description = command.Description;
            Manual = command.Manual;
            InteractiveModeOnly = command.InteractiveModeOnly;

            CommandLineArguments = commandLineArguments;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(" ", CommandLineArguments);
        }
    }
}
