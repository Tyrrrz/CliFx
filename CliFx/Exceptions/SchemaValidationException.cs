using System;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Thrown when a command schema fails validation.
    /// </summary>
    public class SchemaValidationException : CliFxException
    {
        /// <summary>
        /// Initializes an instance of <see cref="SchemaValidationException"/>.
        /// </summary>
        public SchemaValidationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="SchemaValidationException"/>.
        /// </summary>
        public SchemaValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}