using System;
using System.Threading;
using CliFx.Utils;

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
    /// Calling this method makes the command cancellation-aware, which means that sending
    /// the interrupt signal won't immediately terminate the application, but will instead
    /// trigger the associated token, allowing the command to exit early but on its own terms.
    /// </para>
    /// <para>
    /// Note that if the user sends the interrupt signal a second time, the application will
    /// be forcefully terminated without triggering the token.
    /// </para>
    /// </remarks>
    CancellationToken RegisterCancellationHandler();
}

/// <summary>
/// Extensions for <see cref="IConsole" />.
/// </summary>
public static class ConsoleExtensions
{
    /// <summary>
    /// Writes a string to the console's output stream.
    /// </summary>
    public static void Write(this IConsole console, string? value) => console.Output.Write(value);

    /// <summary>
    /// Writes an object to the console's output stream.
    /// </summary>
    public static void Write(this IConsole console, object? value) => console.Output.Write(value);

    /// <summary>
    /// Writes an empty line to the console's output stream.
    /// </summary>
    public static void WriteLine(this IConsole console) => console.Output.WriteLine();

    /// <summary>
    /// Writes a string to the console's output stream, followed by a line terminator.
    /// </summary>
    public static void WriteLine(this IConsole console, string? value) =>
        console.Output.WriteLine(value);

    /// <summary>
    /// Writes an object to the console's output stream, followed by a line terminator.
    /// </summary>
    public static void WriteLine(this IConsole console, object? value) =>
        console.Output.WriteLine(value);

    /// <summary>
    /// Reads a single character from the console's input stream.
    /// </summary>
    public static int Read(this IConsole console) => console.Input.Read();

    /// <summary>
    /// Reads a line from the console's input stream.
    /// </summary>
    public static string? ReadLine(this IConsole console) => console.Input.ReadLine();

    /// <summary>
    /// Sets the specified foreground color and returns an <see cref="IDisposable" />
    /// that will reset the color back to its previous value upon disposal.
    /// </summary>
    public static IDisposable WithForegroundColor(
        this IConsole console,
        ConsoleColor foregroundColor
    )
    {
        var lastColor = console.ForegroundColor;
        console.ForegroundColor = foregroundColor;

        return Disposable.Create(() => console.ForegroundColor = lastColor);
    }

    /// <summary>
    /// Sets the specified background color and returns an <see cref="IDisposable" />
    /// that will reset the color back to its previous value upon disposal.
    /// </summary>
    public static IDisposable WithBackgroundColor(
        this IConsole console,
        ConsoleColor backgroundColor
    )
    {
        var lastColor = console.BackgroundColor;
        console.BackgroundColor = backgroundColor;

        return Disposable.Create(() => console.BackgroundColor = lastColor);
    }

    /// <summary>
    /// Sets the specified foreground and background colors and returns an <see cref="IDisposable" />
    /// that will reset the colors back to their previous values upon disposal.
    /// </summary>
    public static IDisposable WithColors(
        this IConsole console,
        ConsoleColor foregroundColor,
        ConsoleColor backgroundColor
    ) =>
        Disposable.Merge(
            console.WithForegroundColor(foregroundColor),
            console.WithBackgroundColor(backgroundColor)
        );
}
