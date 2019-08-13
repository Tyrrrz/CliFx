using System;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Thrown when a required command option was not set.
    /// </summary>
    public class MissingCommandOptionException : CliFxException
    {
        /// <summary>
        /// Initializes an instance of <see cref="MissingCommandOptionException"/>.
        /// </summary>
        public MissingCommandOptionException()
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="MissingCommandOptionException"/>.
        /// </summary>
        public MissingCommandOptionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="MissingCommandOptionException"/>.
        /// </summary>
        public MissingCommandOptionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}