using System;

namespace CliFx.Exceptions;

/// <summary>
/// Exception thrown within <see cref="CliFx" />.
/// </summary>
public partial class CliFxException(
    string message,
    int exitCode = CliFxException.DefaultExitCode,
    bool showHelp = false,
    Exception? innerException = null
) : Exception(message, innerException)
{
    internal const int DefaultExitCode = 1;

    // When an exception is created without a message, the base Exception class
    // provides a default message that is not very useful.
    // This property is used to identify whether this instance was created with
    // a custom message, so that we can avoid printing the default message.
    internal bool HasCustomMessage { get; } = !string.IsNullOrWhiteSpace(message);

    /// <summary>
    /// Returned exit code.
    /// </summary>
    public int ExitCode { get; } = exitCode;

    /// <summary>
    /// Whether to show the help text before exiting.
    /// </summary>
    public bool ShowHelp { get; } = showHelp;
}

public partial class CliFxException
{
    // Internal errors don't show help because they're meant for the developer and
    // not the end-user of the application.
    internal static CliFxException InternalError(
        string message,
        Exception? innerException = null
    ) => new(message, DefaultExitCode, false, innerException);

    // User errors are typically caused by invalid input and are meant for the end-user,
    // so we want to show help.
    internal static CliFxException UserError(string message, Exception? innerException = null) =>
        new(message, DefaultExitCode, true, innerException);
}
