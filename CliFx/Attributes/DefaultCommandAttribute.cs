using System;

namespace CliFx.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DefaultCommandAttribute : CommandAttribute
    {
        public DefaultCommandAttribute()
            : base(null)
        {
        }
    }
}