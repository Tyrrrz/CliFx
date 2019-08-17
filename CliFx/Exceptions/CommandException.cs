using System;
using CliFx.Internal;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Thrown when a command cannot proceed with normal execution due to error.
    /// Use this exception if you want to specify an exit code to return when the process terminates.
    /// </summary>
    public class CommandException : CliFxException
    {
        /// <summary>
        /// Process exit code.
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandException"/>.
        /// </summary>
        public CommandException(string message, int exitCode, Exception innerException)
            : base(message, innerException)
        {
            ExitCode = exitCode.GuardNotZero(nameof(exitCode));
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandException"/>.
        /// </summary>
        public CommandException(string message, int exitCode)
            : this(message, exitCode,  null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandException"/>.
        /// </summary>
        public CommandException(int exitCode)
            : this(null, exitCode)
        {
        }
    }
}