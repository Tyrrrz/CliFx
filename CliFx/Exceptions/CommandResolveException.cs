using System;

namespace CliFx.Exceptions
{
    public class CommandResolveException : Exception
    {
        public CommandResolveException()
        {
        }

        public CommandResolveException(string message)
            : base(message)
        {
        }

        public CommandResolveException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}