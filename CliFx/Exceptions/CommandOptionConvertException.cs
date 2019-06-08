using System;

namespace CliFx.Exceptions
{
    public class CommandOptionConvertException : Exception
    {
        public CommandOptionConvertException()
        {
        }

        public CommandOptionConvertException(string message)
            : base(message)
        {
        }

        public CommandOptionConvertException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}