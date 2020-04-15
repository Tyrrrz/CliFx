using System;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Thrown when a command cannot proceed with normal execution due to an error.
    /// Use this exception if you want to report an error that occured during execution of a command.
    /// This exception also allows specifying exit code which will be returned to the calling process.
    /// </summary>
    public class CommandException : Exception
    {
        private const int DefaultExitCode = -100;

        /// <summary>
        /// Gets the error display options for this exception.
        /// </summary>
        public CommandErrorDisplayOptions ErrorDisplayOptions { get; }

        /// <summary>
        /// Process exit code.
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandException"/>.
        /// </summary>
        public CommandException(string? message, Exception? innerException, int exitCode = DefaultExitCode,
            CommandErrorDisplayOptions errorDisplayOptions = CommandErrorDisplayOptions.ExceptionMessage)
                : base(message, innerException)
        {
            ErrorDisplayOptions = errorDisplayOptions;
            ExitCode = exitCode != 0
                ? exitCode
                : throw new ArgumentException("Exit code must not be zero in order to signify failure.");
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandException"/>.
        /// </summary>
        public CommandException(string? message, int exitCode = DefaultExitCode, 
            CommandErrorDisplayOptions errorDisplayOptions = CommandErrorDisplayOptions.ExceptionMessage)
                : this(message, null, exitCode, errorDisplayOptions)
        {            
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandException"/>.
        /// </summary>
        public CommandException(string? message, 
            CommandErrorDisplayOptions errorDisplayOptions = CommandErrorDisplayOptions.ExceptionMessage, 
            int exitCode = DefaultExitCode)
                : this(message, null, exitCode, errorDisplayOptions)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandException"/>.
        /// </summary>
        public CommandException(
            CommandErrorDisplayOptions errorDisplayOptions = CommandErrorDisplayOptions.ExceptionMessage,
            int exitCode = DefaultExitCode)
                : this(null, exitCode, errorDisplayOptions)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandException"/>.
        /// </summary>
        public CommandException(int exitCode = DefaultExitCode)
            : this(null, exitCode)
        {
        }        
    }
}
