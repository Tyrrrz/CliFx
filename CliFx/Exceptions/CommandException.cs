using System;

namespace CliFx.Exceptions;

/// <summary>
/// Exception thrown when a command cannot proceed with its normal execution due to an error.
/// Use this exception to report an error to the console and return a specific exit code.
/// </summary>
public class CommandException(
    string message,
    int exitCode = CliFxException.DefaultExitCode,
    bool showHelp = false,
    Exception? innerException = null
) : CliFxException(message, exitCode, showHelp, innerException);
