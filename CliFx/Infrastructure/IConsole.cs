using System;
using System.Threading;
using CliFx.Utils;

namespace CliFx.Infrastructure;

/// <summary>
/// Abstraction for the console layer.
/// </summary>
public interface IConsole
{
    /// <summary>
    /// Input stream (stdin).
    /// </summary>
    ConsoleReader Input { get; }

    /// <summary>
    /// Whether the input stream is redirected.
    /// </summary>
    bool IsInputRedirected { get; }

    /// <summary>
    /// Output stream (stdout).
    /// </summary>
    ConsoleWriter Output { get; }

    /// <summary>
    /// Whether the output stream is redirected.
    /// </summary>
    bool IsOutputRedirected { get; }

    /// <summary>
    /// Error stream (stderr).
    /// </summary>
    ConsoleWriter Error { get; }

    /// <summary>
    /// Whether the error stream is redirected.
    /// </summary>
    bool IsErrorRedirected { get; }

    /// <summary>
    /// Current foreground color.
    /// </summary>
    ConsoleColor ForegroundColor { get; set; }

    /// <summary>
    /// Current background color.
    /// </summary>
    ConsoleColor BackgroundColor { get; set; }

    /// <summary>
    /// Resets foreground and background colors to their default values.
    /// </summary>
    void ResetColor();

    /// <summary>
    /// Cursor left offset.
    /// </summary>
    int CursorLeft { get; set; }

    /// <summary>
    /// Cursor top offset.
    /// </summary>
    int CursorTop { get; set; }

    /// <summary>
    /// Registers a handler for the interrupt signal (Ctrl+C) on the console and returns
    /// a token representing the cancellation request.
    /// Subsequent calls to this method have no side-effects and return the same token.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Calling this method effectively makes the command cancellation-aware, which
    /// means that sending the interrupt signal won't immediately terminate the application,
    /// but will instead trigger a token that the command can use to exit more gracefully.
    /// </para>
    /// <para>
    /// Note that the handler is only respected when the user sends the interrupt signal for the first time.
    /// If the user decides to issue the signal again, the application will terminate immediately
    /// regardless of whether the command is cancellation-aware.
    /// </para>
    /// </remarks>
    CancellationToken RegisterCancellationHandler();

    /// <summary>
    /// Clears the console buffer and corresponding console window of display information.
    /// </summary>
    void Clear();

    /// <summary>
    /// Obtains the next character or function key pressed by the user.
    /// </summary>
    ConsoleKeyInfo ReadKey(bool intercept = false);
}

/// <summary>
/// Extensions for <see cref="IConsole"/>.
/// </summary>
public static class ConsoleExtensions
{
    /// <summary>
    /// Sets the specified foreground color and returns an <see cref="IDisposable"/>
    /// that will reset the color back to its previous value upon disposal.
    /// </summary>
    public static IDisposable WithForegroundColor(this IConsole console, ConsoleColor foregroundColor)
    {
        var lastColor = console.ForegroundColor;
        console.ForegroundColor = foregroundColor;

        return Disposable.Create(() => console.ForegroundColor = lastColor);
    }

    /// <summary>
    /// Sets the specified background color and returns an <see cref="IDisposable"/>
    /// that will reset the color back to its previous value upon disposal.
    /// </summary>
    public static IDisposable WithBackgroundColor(this IConsole console, ConsoleColor backgroundColor)
    {
        var lastColor = console.BackgroundColor;
        console.BackgroundColor = backgroundColor;

        return Disposable.Create(() => console.BackgroundColor = lastColor);
    }

    /// <summary>
    /// Sets the specified foreground and background colors and returns an <see cref="IDisposable"/>
    /// that will reset the colors back to their previous values upon disposal.
    /// </summary>
    public static IDisposable WithColors(
        this IConsole console,
        ConsoleColor foregroundColor,
        ConsoleColor backgroundColor) =>
        Disposable.Merge(
            console.WithForegroundColor(foregroundColor),
            console.WithBackgroundColor(backgroundColor)
        );
}