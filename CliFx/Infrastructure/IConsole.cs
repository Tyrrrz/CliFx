using System;
using System.IO;
using System.Threading;
using CliFx.Utils;

namespace CliFx.Infrastructure
{
    /// <summary>
    /// Abstraction for the console layer.
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
        /// Registers a handler for the interrupt signal (i.e. Ctrl+C) on the console
        /// and returns a token representing the cancellation request.
        ///
        /// You can use this token to perform graceful termination in a command.
        ///
        /// Subsequent calls to this method have no effect and return the original token.
        /// </summary>
        /// <remarks>
        /// Calling this method effectively makes the command cancellation-aware, which
        /// means that it becomes responsible for correctly processing and handling
        /// user's cancellation request.
        ///
        /// If the user sends an interrupt signal before the command receives the cancellation
        /// token, the application will terminate instantly.
        ///
        /// If the user sends a second interrupt signal after the first one, the application
        /// will terminate instantly regardless of whether the command is cancellation-aware.
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

        private static void WriteException(this IConsole console, Exception exception, int indentLevel)
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