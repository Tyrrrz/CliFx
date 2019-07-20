using System;

namespace CliFx.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandOptionAttribute : Attribute
    {
        public string Name { get; }

        public char? ShortName { get; }

        public bool IsRequired { get; set; }

        public string GroupName { get; set; }

        public string Description { get; set; }

        public CommandOptionAttribute(string name, char? shortName)
        {
            Name = name;
            ShortName = shortName;
        }

        public CommandOptionAttribute(string name, char shortName)
            : this(name, (char?) shortName)
        {
        }

        public CommandOptionAttribute(string name)
            : this(name, null)
        {
        }

        public CommandOptionAttribute(char shortName)
            : this(null, shortName)
        {
        }
    }
}