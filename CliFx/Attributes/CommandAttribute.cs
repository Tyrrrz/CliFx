using System;

namespace CliFx.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; }

        public string Description { get; set; }

        public CommandAttribute(string name)
        {
            Name = name;
        }
    }
}