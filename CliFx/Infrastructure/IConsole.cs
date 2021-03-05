using System;
using System.IO;
using System.Threading;
using CliFx.Utils;

namespace CliFx.Infrastructure
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
        /// Defers the application termination in case of a cancellation request and returns the token that represents it.
        /// Subsequent calls to this method return the same token.
        /// </summary>
        /// <remarks>
        /// When working with <see cref="SystemConsole"/>:<br/>
        /// - Cancellation can be requested by the user by pressing Ctrl+C.<br/>
        /// - Cancellation can only be deferred once, subsequent requests to cancel by the user will result in instant termination.<br/>
        /// - Any code executing prior to calling this method is not cancellation-aware and as such will terminate instantly when cancellation is requested.
        /// </remarks>
        CancellationToken RegisterCancellation();
    }

    /// <summary>
    /// Extensions for <see cref="IConsole"/>.
    /// </summary>
    public static class ConsoleExtensions
    {
        /// <summary>
        /// Sets the specified foreground color and returns an <see cref="IDisposable"/>
        /// that will reset the color back to its previous value.
        /// </summary>
        public static IDisposable WithForegroundColor(this IConsole console, ConsoleColor foregroundColor)
        {
            var lastColor = console.ForegroundColor;
            console.ForegroundColor = foregroundColor;

            return Disposable.Create(() => console.ForegroundColor = lastColor);
        }

        /// <summary>
        /// Sets the specified background color and returns an <see cref="IDisposable"/>
        /// that will reset the color back to its previous value.
        /// </summary>
        public static IDisposable WithBackgroundColor(this IConsole console, ConsoleColor backgroundColor)
        {
            var lastColor = console.BackgroundColor;
            console.BackgroundColor = backgroundColor;

            return Disposable.Create(() => console.BackgroundColor = lastColor);
        }

        /// <summary>
        /// Sets the specified foreground and background colors and returns an <see cref="IDisposable"/>
        /// that will reset the colors back to their previous values.
        /// </summary>
        public static IDisposable WithColors(
            this IConsole console,
            ConsoleColor foregroundColor,
            ConsoleColor backgroundColor)
        {
            var lastForegroundColor = console.ForegroundColor;
            console.ForegroundColor = foregroundColor;

            var lastBackgroundColor = console.BackgroundColor;
            console.BackgroundColor = backgroundColor;

            return Disposable.Create(() =>
            {
                console.ForegroundColor = lastForegroundColor;
                console.BackgroundColor = lastBackgroundColor;
            });
        }

        private static void WriteException(
            this IConsole console,
            Exception exception,
            int indentLevel)
        {
            var exceptionType = exception.GetType();

            var indentationShared = new string(' ', 4 * indentLevel);
            var indentationLocal = new string(' ', 2);

            // Fully qualified exception type
            console.Error.Write(indentationShared);

            using (console.WithForegroundColor(ConsoleColor.DarkGray))
                console.Error.Write(exceptionType.Namespace + ".");

            using (console.WithForegroundColor(ConsoleColor.White))
                console.Error.Write(exceptionType.Name);

            console.Error.Write(": ");

            // Exception message
            using (console.WithForegroundColor(ConsoleColor.Red))
                console.Error.WriteLine(exception.Message);

            // Recurse into inner exceptions
            if (exception.InnerException is not null)
            {
                console.WriteException(exception.InnerException, indentLevel + 1);
            }

            // Try to parse and pretty-print the stack trace
            try
            {
                foreach (var stackFrame in StackFrame.ParseMany(exception.StackTrace))
                {
                    console.Error.Write(indentationShared + indentationLocal);
                    console.Error.Write("at ");

                    // "CliFx.Demo.Commands.BookAddCommand."
                    using (console.WithForegroundColor(ConsoleColor.DarkGray))
                        console.Error.Write(stackFrame.ParentType + ".");

                    // "ExecuteAsync"
                    using (console.WithForegroundColor(ConsoleColor.Yellow))
                        console.Error.Write(stackFrame.MethodName);

                    console.Error.Write("(");

                    for (var i = 0; i < stackFrame.Parameters.Count; i++)
                    {
                        var parameter = stackFrame.Parameters[i];

                        // Separator
                        if (i > 0)
                        {
                            console.Error.Write(", ");
                        }

                        // "IConsole"
                        using (console.WithForegroundColor(ConsoleColor.Blue))
                            console.Error.Write(parameter.Type);

                        if (!string.IsNullOrWhiteSpace(parameter.Name))
                        {
                            console.Error.Write(" ");

                            // "console"
                            using (console.WithForegroundColor(ConsoleColor.White))
                                console.Error.Write(parameter.Name);
                        }
                    }

                    console.Error.Write(") ");

                    // Location
                    if (!string.IsNullOrWhiteSpace(stackFrame.FilePath))
                    {
                        console.Error.Write("in");
                        console.Error.Write("\n" + indentationShared + indentationLocal + indentationLocal);

                        // "E:\Projects\Softdev\CliFx\CliFx.Demo\Commands\"
                        using (console.WithForegroundColor(ConsoleColor.DarkGray))
                        {
                            var stackFrameDirectoryPath = Path.GetDirectoryName(stackFrame.FilePath);
                            console.Error.Write(stackFrameDirectoryPath + Path.DirectorySeparatorChar);
                        }

                        // "BookAddCommand.cs"
                        using (console.WithForegroundColor(ConsoleColor.Yellow))
                        {
                            var stackFrameFileName = Path.GetFileName(stackFrame.FilePath);
                            console.Error.Write(stackFrameFileName);
                        }

                        if (!string.IsNullOrWhiteSpace(stackFrame.LineNumber))
                        {
                            console.Error.Write(":");

                            // "35"
                            using (console.WithForegroundColor(ConsoleColor.Blue))
                                console.Error.Write(stackFrame.LineNumber);
                        }
                    }

                    console.Error.WriteLine();
                }
            }
            // If any point of parsing has failed - print the stack trace without any formatting
            catch
            {
                console.Error.WriteLine(exception.StackTrace);
            }
        }

        // Should this be public?
        internal static void WriteException(this IConsole console, Exception exception) =>
            console.WriteException(exception, 0);
    }
}