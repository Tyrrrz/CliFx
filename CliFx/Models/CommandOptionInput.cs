using System.Collections.Generic;

namespace CliFx.Models
{
    public class CommandOptionInput
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
    }
}