using System.Collections.Generic;
using System.Text;
using CliFx.Internal;

namespace CliFx.Models
{
    /// <summary>
    /// Parsed option from command line input.
    /// </summary>
    public partial class CommandOptionInput
    {
        /// <summary>
        /// Specified option alias.
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// Specified values.
        /// </summary>
        public IReadOnlyList<string> Values { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionInput"/>.
        /// </summary>
        public CommandOptionInput(string alias, IReadOnlyList<string> values)
        {
            Alias = alias.GuardNotNull(nameof(alias));
            Values = values.GuardNotNull(nameof(values));
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionInput"/>.
        /// </summary>
        public CommandOptionInput(string alias, string value)
            : this(alias, new[] {value})
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandOptionInput"/>.
        /// </summary>
        public CommandOptionInput(string alias)
            : this(alias, EmptyValues)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var buffer = new StringBuilder();

            buffer.Append(Alias.Length > 1 ? "--" : "-");
            buffer.Append(Alias);

            foreach (var value in Values)
            {
                buffer.AppendIfNotEmpty(' ');

                var isEscaped = value.Contains(" ");

                if (isEscaped)
                    buffer.Append('"');

                buffer.Append(value);

                if (isEscaped)
                    buffer.Append('"');
            }

            return buffer.ToString();
        }
    }

    public partial class CommandOptionInput
    {
        private static readonly IReadOnlyList<string> EmptyValues = new string[0];
    }
}