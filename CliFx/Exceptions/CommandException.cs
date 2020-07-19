﻿using System;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Thrown when a command cannot proceed with normal execution due to an error.
    /// Use this exception if you want to report an error that occured during the execution of a command.
    /// This exception also allows specifying exit code which will be returned to the calling process.
    /// </summary>
    public class CommandException : Exception
    {
        private const int DefaultExitCode = 1;

        private readonly bool _isMessageSet;

        /// <summary>
        /// Returns an exit code associated with this exception.
        /// On Unix systems an exit code is 8-bit unsigned integer so it's strongly recommended to use exit codes between 1 and 255
        /// otherwise it may overflow and yield unexpected results.
        /// </summary>
        public int ExitCode { get; }

        /// <summary>
        /// Whether to show the help text after handling this exception.
        /// </summary>
        public bool ShowHelp { get; }

        /// <summary>
        /// Initializes an instance of <see cref="CommandException"/>.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="exitCode">
        /// The exit code associated with this exception.
        /// On Unix systems an exit code is 8-bit unsigned integer so it's strongly recommended to use exit codes between 1 and 255
        /// otherwise it may overflow and yield unexpected results.
        /// </param>
        /// <param name="showHelp">Whether to show the help text after handling this exception.</param>
        public CommandException(string? message, Exception? innerException, int exitCode = DefaultExitCode, bool showHelp = false)
            : base(message, innerException)
        {
            ExitCode = exitCode;
            ShowHelp = showHelp;

            // Message property has a fallback so it's never empty, hence why we need this check
            _isMessageSet = !string.IsNullOrWhiteSpace(message);
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandException"/>.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="exitCode">
        /// The exit code associated with this exception.
        /// On Unix systems an exit code is 8-bit unsigned integer so it's strongly recommended to use exit codes between 1 and 255
        /// otherwise it may overflow and yield unexpected results.
        /// </param>
        /// <param name="showHelp">Whether to show the help text after handling this exception.</param>
        public CommandException(string? message, int exitCode = DefaultExitCode, bool showHelp = false)
            : this(message, null, exitCode, showHelp)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="CommandException"/>.
        /// </summary>
        /// <param name="exitCode">
        /// The exit code associated with this exception.
        /// On Unix systems an exit code is 8-bit unsigned integer so it's strongly recommended to use exit codes between 1 and 255
        /// otherwise it may overflow and yield unexpected results.
        /// </param>
        /// <param name="showHelp">Whether to show the help text after handling this exception.</param>
        public CommandException(int exitCode = DefaultExitCode, bool showHelp = false)
            : this(null, exitCode, showHelp)
        {
        }

        /// <inheritdoc />
        public override string ToString() => _isMessageSet
            ? Message
            : base.ToString();
    }
}