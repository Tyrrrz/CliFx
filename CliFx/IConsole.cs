using System;
using System.IO;
using System.Threading;

namespace CliFx
{
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
        /// Provides a token that signals when application cancellation is requested.
        /// Subsequent calls return the same token.
        /// When working with system console, the user can request cancellation by issuing an interrupt signal (Ctrl+C).
        /// </summary>
        CancellationToken GetCancellationToken();
    }
}