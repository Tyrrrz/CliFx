using System.Collections.Generic;
using System.Text;

namespace CliFx.Models
{
    /// <summary>
    /// Parsed option from command line input.
    /// </summary>
    public class CommandOptionInput
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
            Alias = alias;
            Values = values;
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
            : this(alias, new string[0])
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
                buffer.Append(' ');

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
}