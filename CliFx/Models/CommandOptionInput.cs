using System.Collections.Generic;

namespace CliFx.Models
{
    public class CommandOptionInput
    {
        public string Name { get; }

        public IReadOnlyList<string> Values { get; }

        public CommandOptionInput(string name, IReadOnlyList<string> values)
        {
            Name = name;
            Values = values;
        }

        public CommandOptionInput(string name, string value)
            : this(name, new[] {value})
        {
        }

        public CommandOptionInput(string name)
            : this(name, new string[0])
        {
        }
    }
}