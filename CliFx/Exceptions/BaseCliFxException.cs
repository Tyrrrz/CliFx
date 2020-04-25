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
        /// Represents the default exit code assigned to exceptions in CliFx.
        /// </summary>
        protected const int DefaultExitCode = -100;

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
        /// Returns an exit code associated with this exception.
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Initializes an instance of <see cref="BaseCliFxException"/>.
        /// </summary>
        protected BaseCliFxException(string? message, int exitCode = DefaultExitCode, bool showHelp = false)
            : this(message, null, exitCode, showHelp)
        {            
        }

        /// <summary>
        /// Initializes an instance of <see cref="BaseCliFxException"/>.
        /// </summary>
        protected BaseCliFxException(string? message, Exception? innerException, int exitCode = DefaultExitCode, bool showHelp = false)
            : base(message, innerException)
        {
            ExitCode = exitCode != 0
                ? exitCode
                : throw new ArgumentException("Exit code must not be zero in order to signify failure.");
            HasMessage = string.IsNullOrWhiteSpace(message) ? false : true;
            ShowHelp = showHelp;
        }
    }
}