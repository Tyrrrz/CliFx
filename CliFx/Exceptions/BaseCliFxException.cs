using System;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Provides the base functionality for exceptions thrown within CliFx
    /// or from one of its commands.
    /// </summary>
    public abstract class BaseCliFxException : Exception
    {
        /// <summary>
        /// Whether to show the help text after handling this exception.
        /// </summary>
        public bool ShowHelp { get; }

        /// <summary>
        /// Whether this exception was constructed with a message.
        /// </summary>
        /// <remarks>
        /// We cannot check against the 'Message' property because it will always return 
        /// a default message if it was constructed with a null value or is currently null.
        /// </remarks>
        public bool HasMessage { get; }

        /// <summary>
        /// Initializes an instance of <see cref="BaseCliFxException"/>.
        /// </summary>
        protected BaseCliFxException(string? message, bool showHelp = false)
            : this(message, null, showHelp)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="BaseCliFxException"/>.
        /// </summary>
        protected BaseCliFxException(string? message, Exception? innerException, bool showHelp = false)
            : base(message, innerException)
        {
            HasMessage = string.IsNullOrWhiteSpace(message) ? false : true;
            ShowHelp = showHelp;
        }
    }
}