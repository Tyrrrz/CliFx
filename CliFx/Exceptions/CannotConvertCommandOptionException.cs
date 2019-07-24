using System;

namespace CliFx.Exceptions
{
    public class CannotConvertCommandOptionException : Exception
    {
        public CannotConvertCommandOptionException()
        {
        }

        public CannotConvertCommandOptionException(string message)
            : base(message)
        {
        }

        public CannotConvertCommandOptionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}