using System;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Thrown when a command schema fails validation.
    /// </summary>
    public class InvalidCommandSchemaException : CliFxException
    {
        /// <summary>
        /// Initializes an instance of <see cref="InvalidCommandSchemaException"/>.
        /// </summary>
        public InvalidCommandSchemaException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="InvalidCommandSchemaException"/>.
        /// </summary>
        public InvalidCommandSchemaException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}