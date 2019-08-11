using System;

namespace CliFx.Exceptions
{
    public class MissingCommandOptionException : CliFxException
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