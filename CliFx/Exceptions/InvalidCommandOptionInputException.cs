using System;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Thrown when a command option can't be converted to target type specified in its schema.
    /// </summary>
    public class InvalidCommandOptionInputException : CliFxException
    {
        /// <summary>
        /// Initializes an instance of <see cref="InvalidCommandOptionInputException"/>.
        /// </summary>
        public InvalidCommandOptionInputException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="InvalidCommandOptionInputException"/>.
        /// </summary>
        public InvalidCommandOptionInputException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}