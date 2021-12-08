using System;

namespace CliFx.Exceptions;

/// <summary>
/// Exception thrown when a command cannot proceed with its normal execution due to an error.
/// Use this exception to report an error to the console and return a specific exit code.
/// </summary>
public class CommandException : CliFxException
{
    /// <summary>
    /// Initializes an instance of <see cref="CommandException"/>.
    /// </summary>
    public CommandException(
        string message,
        int exitCode = DefaultExitCode,
        bool showHelp = false,
        Exception? innerException = null)
        : base(message, exitCode, showHelp, innerException)
    {
    }
}