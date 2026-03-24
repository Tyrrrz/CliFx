using System;

namespace CliFx;

/// <summary>
/// Exception thrown when a command cannot proceed with its normal execution due to an error.
/// Use this exception to report an error to the console, return a specific exit code,
/// and optionally show the help text for the executed command.
/// </summary>
public class CommandException(
    string message,
    int exitCode = CliFxException.DefaultExitCode,
    bool showHelp = false,
    Exception? innerException = null
) : CliFxException(message, exitCode, showHelp, innerException);
