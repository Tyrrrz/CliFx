using System;

namespace CliFx.Exceptions
{
    public class MissingCommandOptionException : Exception
    {
        public MissingCommandOptionException()
        {
        }

        public MissingCommandOptionException(string message)
            : base(message)
        {
        }

        public MissingCommandOptionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}