namespace CliFx
{
    using System;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// Abstraction for interacting with the console.
    /// </summary>
    public interface IConsole
    {
        /// <summary>
        /// Input stream (stdin).
        /// </summary>
        StreamReader Input { get; }

        /// <summary>
        /// Whether the input stream is redirected.
        /// </summary>
        bool IsInputRedirected { get; }

        /// <summary>
        /// Output stream (stdout).
        /// </summary>
        StreamWriter Output { get; }

        /// <summary>
        /// Whether the output stream is redirected.
        /// </summary>
        bool IsOutputRedirected { get; }

        /// <summary>
        /// Error stream (stderr).
        /// </summary>
        StreamWriter Error { get; }

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
        /// Clears the console.
        /// </summary>
        void Clear();

        /// <summary>
        /// Resets foreground and background color to default values.
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
        /// Window width.
        /// </summary>
        int WindowWidth { get; set; }

        /// <summary>
        /// Window height.
        /// </summary>
        int WindowHeight { get; set; }

        /// <summary>
        /// Window buffer width.
        /// </summary>
        int BufferWidth { get; set; }

        /// <summary>
        /// Window buffer height.
        /// </summary>
        int BufferHeight { get; set; }

        /// <summary>
        /// Defers the application termination in case of a cancellation request and returns the token that represents it.
        /// Subsequent calls to this method return the same token.
        /// </summary>
        /// <remarks>
        /// When working with <see cref="SystemConsole"/>:<br/>
        /// - Cancellation can be requested by the user by pressing Ctrl+C.<br/>
        /// - Cancellation can only be deferred once, subsequent requests to cancel by the user will result in instant termination.<br/>
        /// - Any code executing prior to calling this method is not cancellation-aware and as such will terminate instantly when cancellation is requested.
        /// </remarks>
        CancellationToken GetCancellationToken();
    }

    /// <summary>
    /// Extensions for <see cref="IConsole"/>.
    /// </summary>
    public static class ConsoleExtensions
    {
        /// <summary>
        /// Sets console foreground color, executes specified action, and sets the color back to the original value.
        /// </summary>
        public static void WithForegroundColor(this IConsole console, ConsoleColor foregroundColor, Action action)
        {
            ConsoleColor lastColor = console.ForegroundColor;
            console.ForegroundColor = foregroundColor;

            action();

            console.ForegroundColor = lastColor;
        }

        /// <summary>
        /// Sets console background color, executes specified action, and sets the color back to the original value.
        /// </summary>
        public static void WithBackgroundColor(this IConsole console, ConsoleColor backgroundColor, Action action)
        {
            ConsoleColor lastColor = console.BackgroundColor;
            console.BackgroundColor = backgroundColor;

            action();

            console.BackgroundColor = lastColor;
        }

        /// <summary>
        /// Sets console foreground and background colors, executes specified action, and sets the colors back to the original values.
        /// </summary>
        public static void WithColors(this IConsole console, ConsoleColor foregroundColor, ConsoleColor backgroundColor, Action action)
        {
            console.WithForegroundColor(foregroundColor, () => console.WithBackgroundColor(backgroundColor, action));
        }
    }
}