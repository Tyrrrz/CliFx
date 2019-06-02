using System;

namespace CliFx.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandOptionAttribute : Attribute
    {
        public string Name { get; }

        public char ShortName { get; set; }

        public bool IsRequired { get; set; }

        public string Description { get; set; }

        public CommandOptionAttribute(string name)
        {
            Name = name;
        }
    }
}