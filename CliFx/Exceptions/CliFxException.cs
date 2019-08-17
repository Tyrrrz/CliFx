using System;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Domain exception thrown within CliFx.
    /// </summary>
    public abstract class CliFxException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="CliFxException"/>.
        /// </summary>
        protected CliFxException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CliFxException"/>.
        /// </summary>
        protected CliFxException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}