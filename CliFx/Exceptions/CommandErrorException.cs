using System;
using CliFx.Internal;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Thrown when a command cannot proceed with normal execution due to error.
    /// Use this exception if you want to specify an exit code to use when the process terminates.
    /// </summary>
    public class CommandErrorException : CliFxException
    {
        /// <summary>
        /// Process exit code.
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandErrorException"/>.
        /// </summary>
        public CommandErrorException(int exitCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ExitCode = exitCode.GuardNotZero(nameof(exitCode));
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandErrorException"/>.
        /// </summary>
        public CommandErrorException(int exitCode, Exception innerException)
            : this(exitCode, null, innerException)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandErrorException"/>.
        /// </summary>
        public CommandErrorException(int exitCode, string message)
            : this(exitCode, message, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandErrorException"/>.
        /// </summary>
        public CommandErrorException(int exitCode)
            : this(exitCode, null, null)
        {
        }
    }
}