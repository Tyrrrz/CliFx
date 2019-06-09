using System.Collections.Generic;

namespace CliFx.Models
{
    public class CommandOption
    {
        public string Name { get; }

        public IReadOnlyList<string> Values { get; }

        public CommandOption(string name, IReadOnlyList<string> values)
        {
            Name = name;
            Values = values;
        }

        public CommandOption(string name, string value)
            : this(name, new[] {value})
        {
        }

        public CommandOption(string name)
            : this(name, new string[0])
        {
        }
    }
}