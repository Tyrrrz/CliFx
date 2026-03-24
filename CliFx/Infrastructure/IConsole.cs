using System;
using System.Threading;

namespace CliFx.Infrastructure;

/// <summary>
/// Abstraction for interacting with the console layer.
/// </summary>
public interface IConsole
{
    /// <summary>
    /// Console's standard input stream.
    /// </summary>
    ConsoleReader Input { get; }

    /// <summary>
    /// Whether the input stream has been redirected.
    /// </summary>
    bool IsInputRedirected { get; }

    /// <summary>
    /// Console's standard output stream.
    /// </summary>
    ConsoleWriter Output { get; }

    /// <summary>
    /// Whether the output stream has been redirected.
    /// </summary>
    bool IsOutputRedirected { get; }

    /// <summary>
    /// Console's standard error stream.
    /// </summary>
    ConsoleWriter Error { get; }

    /// <summary>
    /// Whether the error stream has been redirected.
    /// </summary>
    bool IsErrorRedirected { get; }

    /// <summary>
    /// Gets or sets the current foreground color of the console.
    /// </summary>
    ConsoleColor ForegroundColor { get; set; }

    /// <summary>
    /// Gets or sets the current background color of the console.
    /// </summary>
    ConsoleColor BackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the width of the console window.
    /// </summary>
    int WindowWidth { get; set; }

    /// <summary>
    /// Gets or sets the height of the console window.
    /// </summary>
    int WindowHeight { get; set; }

    /// <summary>
    /// Gets or sets the column position of the cursor within the buffer area.
    /// </summary>
    int CursorLeft { get; set; }

    /// <summary>
    /// Gets or sets the row position of the cursor within the buffer area.
    /// </summary>
    int CursorTop { get; set; }

    /// <summary>
    /// Obtains the next character or function key pressed by the user.
    /// </summary>
    ConsoleKeyInfo ReadKey(bool intercept = false);

    /// <summary>
    /// Sets the foreground and background console colors to their defaults.
    /// </summary>
    void ResetColor();

    /// <summary>
    /// Clears the console buffer and corresponding console window of display information.
    /// </summary>
    void Clear();

    /// <summary>
    /// Registers a handler for the interrupt signal (Ctrl+C) on the console and returns
    /// the token that represents the associated cancellation request.
    /// Subsequent calls to this method have no side effects and return the same token.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Calling this method makes the command cancellation-aware, which means that an interrupt
    /// signal will no longer immediately terminate the application, but will instead trigger
    /// the associated token, allowing the command to exit early on its own terms.
    /// </para>
    /// <para>
    /// The cancellation token is only respected the first time the user sends an interrupt signal.
    /// If the user sends an interrupt signal again, the cancellation token will be ignored and the
    /// application will immediately terminate.
    /// </para>
    /// <para>
    /// Note that the above semantics are not enforced by <see cref="FakeConsole" /> and <see cref="FakeInMemoryConsole" />.
    /// </para>
    /// </remarks>
    CancellationToken RegisterCancellationHandler();
}
