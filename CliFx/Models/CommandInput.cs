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
        /// Specified directives.
        /// </summary>
        public IReadOnlyList<string> Directives { get; }

        /// <summary>
        /// Specified options.
        /// </summary>
        public IReadOnlyList<CommandOptionInput> Options { get; }

        /// <summary>
        /// Environment variables available when the command was parsed
        /// </summary>
        public IReadOnlyDictionary<string, string> EnvironmentVariables { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(string commandName, IReadOnlyList<string> directives, IReadOnlyList<CommandOptionInput> options, IReadOnlyDictionary<string, string> environmentVariables)
        {
            CommandName = commandName; // can be null
            Directives = directives.GuardNotNull(nameof(directives));
            Options = options.GuardNotNull(nameof(options));
            EnvironmentVariables = environmentVariables.GuardNotNull(nameof(environmentVariables));
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(string commandName, IReadOnlyList<string> directives, IReadOnlyList<CommandOptionInput> options)
            : this(commandName, directives, options, EmptyEnvironmentVariables)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(string commandName, IReadOnlyList<CommandOptionInput> options, IReadOnlyDictionary<string, string> environmentVariables)
            : this(commandName, EmptyDirectives, options, environmentVariables)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(string commandName, IReadOnlyList<CommandOptionInput> options)
            : this(commandName, EmptyDirectives, options)
        {
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
            : this(commandName, EmptyOptions)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var buffer = new StringBuilder();

            if (!CommandName.IsNullOrWhiteSpace())
                buffer.Append(CommandName);

            foreach (var directive in Directives)
            {
                buffer.AppendIfNotEmpty(' ');
                buffer.Append(directive);
            }

            foreach (var option in Options)
            {
                buffer.AppendIfNotEmpty(' ');
                buffer.Append(option);
            }

            return buffer.ToString();
        }
    }

    public partial class CommandInput
    {
        private static readonly IReadOnlyList<string> EmptyDirectives = new string[0];
        private static readonly IReadOnlyList<CommandOptionInput> EmptyOptions = new CommandOptionInput[0];
        private static readonly IReadOnlyDictionary<string, string> EmptyEnvironmentVariables = new Dictionary<string, string>();

        /// <summary>
        /// Empty input.
        /// </summary>
        public static CommandInput Empty { get; } = new CommandInput(EmptyOptions);
    }
}