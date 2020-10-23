using System;
using System.IO;
using System.Threading;
using CliFx.Internal;

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
        public static void WithForegroundColor(
            this IConsole console,
            ConsoleColor foregroundColor,
            Action action)
        {
            var lastColor = console.ForegroundColor;
            console.ForegroundColor = foregroundColor;

            action();

            console.ForegroundColor = lastColor;
        }

        /// <summary>
        /// Sets console background color, executes specified action, and sets the color back to the original value.
        /// </summary>
        public static void WithBackgroundColor(
            this IConsole console,
            ConsoleColor backgroundColor,
            Action action)
        {
            var lastColor = console.BackgroundColor;
            console.BackgroundColor = backgroundColor;

            action();

            console.BackgroundColor = lastColor;
        }

        /// <summary>
        /// Sets console foreground and background colors, executes specified action, and sets the colors back to the original values.
        /// </summary>
        public static void WithColors(
            this IConsole console,
            ConsoleColor foregroundColor,
            ConsoleColor backgroundColor,
            Action action) =>
            console.WithForegroundColor(foregroundColor, () => console.WithBackgroundColor(backgroundColor, action));

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
            console.WithForegroundColor(ConsoleColor.DarkGray, () =>
                console.Error.Write(exceptionType.Namespace + ".")
            );
            console.WithForegroundColor(ConsoleColor.White, () =>
                console.Error.Write(exceptionType.Name)
            );
            console.Error.Write(": ");

            // Exception message
            console.WithForegroundColor(ConsoleColor.Red, () => console.Error.WriteLine(exception.Message));

            // Recurse into inner exceptions
            if (exception.InnerException != null)
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
                    console.WithForegroundColor(ConsoleColor.DarkGray, () =>
                        console.Error.Write(stackFrame.ParentType + ".")
                    );

                    // "ExecuteAsync"
                    console.WithForegroundColor(ConsoleColor.Yellow, () =>
                        console.Error.Write(stackFrame.MethodName)
                    );

                    console.Error.Write("(");

                    for (var i = 0; i < stackFrame.Parameters.Count; i++)
                    {
                        var parameter = stackFrame.Parameters[i];

                        // "IConsole"
                        console.WithForegroundColor(ConsoleColor.Blue, () =>
                            console.Error.Write(parameter.Type)
                        );

                        if (!string.IsNullOrWhiteSpace(parameter.Name))
                        {
                            console.Error.Write(" ");

                            // "console"
                            console.WithForegroundColor(ConsoleColor.White, () =>
                                console.Error.Write(parameter.Name)
                            );
                        }

                        // Separator
                        if (stackFrame.Parameters.Count > 1 && i < stackFrame.Parameters.Count - 1)
                        {
                            console.Error.Write(", ");
                        }
                    }

                    console.Error.Write(") ");

                    // Location
                    if (!string.IsNullOrWhiteSpace(stackFrame.FilePath))
                    {
                        console.Error.Write("in");
                        console.Error.Write("\n" + indentationShared + indentationLocal + indentationLocal);

                        // "E:\Projects\Softdev\CliFx\CliFx.Demo\Commands\"
                        var stackFrameDirectoryPath = Path.GetDirectoryName(stackFrame.FilePath);
                        console.WithForegroundColor(ConsoleColor.DarkGray, () =>
                            console.Error.Write(stackFrameDirectoryPath + Path.DirectorySeparatorChar)
                        );

                        // "BookAddCommand.cs"
                        var stackFrameFileName = Path.GetFileName(stackFrame.FilePath);
                        console.WithForegroundColor(ConsoleColor.Yellow, () =>
                            console.Error.Write(stackFrameFileName)
                        );

                        if (!string.IsNullOrWhiteSpace(stackFrame.LineNumber))
                        {
                            console.Error.Write(":");

                            // "35"
                            console.WithForegroundColor(ConsoleColor.Blue, () =>
                                console.Error.Write(stackFrame.LineNumber)
                            );
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

        //Should this be public?
        internal static void WriteException(
            this IConsole console,
            Exception exception) =>
            console.WriteException(exception, 0);
    }
}