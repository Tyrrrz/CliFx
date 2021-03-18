using System;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Exception thrown when there is an error during application execution.
    /// </summary>
    public partial class CliFxException : Exception
    {
        internal const int DefaultExitCode = 1;

        // Regular `exception.Message` never returns null, even if
        // it hasn't been set.
        internal string? ActualMessage { get; }

        /// <summary>
        /// Returned exit code.
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Whether to show the help text before exiting.
        /// </summary>
        public bool ShowHelp { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CliFxException"/>.
        /// </summary>
        public CliFxException(
            string message,
            int exitCode = DefaultExitCode,
            bool showHelp = false,
            Exception? innerException = null)
            : base(message, innerException)
        {
            ActualMessage = message;
            ExitCode = exitCode;
            ShowHelp = showHelp;
        }
    }

    public partial class CliFxException
    {
        // Internal errors don't show help because they're meant for the developer
        // and not the end-user of the application.
        internal static CliFxException InternalError(string message, Exception? innerException = null) =>
            new(message, DefaultExitCode, false, innerException);

        // User errors are typically caused by invalid input and they're meant for
        // the end-user, so we want to show help.
        internal static CliFxException UserError(string message, Exception? innerException = null) =>
            new(message, DefaultExitCode, true, innerException);
    }
}