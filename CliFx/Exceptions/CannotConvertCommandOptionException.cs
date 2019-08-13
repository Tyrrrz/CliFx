using System;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Thrown when an input command option can't be converted.
    /// </summary>
    public class CannotConvertCommandOptionException : CliFxException
    {
        /// <summary>
        /// Initializes an instance of <see cref="CannotConvertCommandOptionException"/>.
        /// </summary>
        public CannotConvertCommandOptionException()
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CannotConvertCommandOptionException"/>.
        /// </summary>
        public CannotConvertCommandOptionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CannotConvertCommandOptionException"/>.
        /// </summary>
        public CannotConvertCommandOptionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}