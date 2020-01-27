using System.Collections.Generic;
using System.Text;
using CliFx.Internal;

namespace CliFx.Domain
{
    internal class CommandOptionInput
    {
        public string Alias { get; }

        public IReadOnlyList<string> Values { get; }

        public CommandOptionInput(string alias, IReadOnlyList<string> values)
        {
            Alias = alias;
            Values = values;
        }

        public CommandOptionInput(string alias, string value)
            : this(alias, new[] {value})
        {
        }

        public CommandOptionInput(string alias)
            : this(alias, new string[0])
        {
        }

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
}