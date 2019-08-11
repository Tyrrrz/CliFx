using System;

namespace CliFx.Exceptions
{
    public abstract class CliFxException : Exception
    {
        protected CliFxException(string message)
            : base(message)
        {
        }

        protected CliFxException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected CliFxException()
        {
        }
    }
}