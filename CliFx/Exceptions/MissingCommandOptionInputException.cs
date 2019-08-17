using System;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Thrown when a required command option was not set.
    /// </summary>
    public class MissingCommandOptionInputException : CliFxException
    {
        /// <summary>
        /// Initializes an instance of <see cref="MissingCommandOptionInputException"/>.
        /// </summary>
        public MissingCommandOptionInputException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="MissingCommandOptionInputException"/>.
        /// </summary>
        public MissingCommandOptionInputException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}