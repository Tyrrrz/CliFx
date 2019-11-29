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
        /// Specified arguments.
        /// </summary>
        public IReadOnlyList<string> Arguments { get; }

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
        public CommandInput(IReadOnlyList<string> arguments, IReadOnlyList<string> directives, IReadOnlyList<CommandOptionInput> options,
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            Arguments = arguments;
            Directives = directives;
            Options = options;
            EnvironmentVariables = environmentVariables;
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(IReadOnlyList<string> arguments, IReadOnlyList<string> directives, IReadOnlyList<CommandOptionInput> options)
            : this(arguments, directives, options, EmptyEnvironmentVariables)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(IReadOnlyList<string> arguments, IReadOnlyList<CommandOptionInput> options, IReadOnlyDictionary<string, string> environmentVariables)
            : this(arguments, EmptyDirectives, options, environmentVariables)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(IReadOnlyList<string> arguments, IReadOnlyList<CommandOptionInput> options)
            : this(arguments, EmptyDirectives, options)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(IReadOnlyList<CommandOptionInput> options)
            : this(new string[0], options)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandInput"/>.
        /// </summary>
        public CommandInput(IReadOnlyList<string> arguments)
            : this(arguments, EmptyOptions)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var buffer = new StringBuilder();

            foreach (var argument in Arguments)
            {
                buffer.AppendIfNotEmpty(' ');
                buffer.Append(argument);
            }

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