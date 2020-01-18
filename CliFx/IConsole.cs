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
        TextReader Input { get; }

        /// <summary>
        /// Whether the input stream is redirected.
        /// </summary>
        bool IsInputRedirected { get; }

        /// <summary>
        /// Output stream (stdout).
        /// </summary>
        TextWriter Output { get; }

        /// <summary>
        /// Whether the output stream is redirected.
        /// </summary>
        bool IsOutputRedirected { get; }

        /// <summary>
        /// Error stream (stderr).
        /// </summary>
        TextWriter Error { get; }

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
        /// Provides a token that signals when application cancellation is requested.
        /// When working with system console, the cancellation happens when the user issues an interrupt signal (Ctrl+C).
        /// Subsequent calls return the same token.
        /// </summary>
        CancellationToken GetCancellationToken();
    }
}