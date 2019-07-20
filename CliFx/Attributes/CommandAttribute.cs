using System;
using CliFx.Internal;

namespace CliFx.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; }

        public string Description { get; set; }

        public bool IsDefault => Name.IsNullOrWhiteSpace();

        public CommandAttribute(string name)
        {
            Name = name;
        }

        public CommandAttribute()
            : this(null)
        {
        }
    }
}