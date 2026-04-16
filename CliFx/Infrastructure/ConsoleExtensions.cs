using System;
using PowerKit;

namespace CliFx.Infrastructure;

/// <summary>
/// Extensions for <see cref="IConsole" />.
/// </summary>
public static class ConsoleExtensions
{
    /// <inheritdoc cref="ConsoleExtensions" />
    extension(IConsole console)
    {
        /// <summary>
        /// Writes a string to the console's output stream.
        /// </summary>
        public void Write(string? value) => console.Output.Write(value);

        /// <summary>
        /// Writes an object to the console's output stream.
        /// </summary>
        public void Write(object? value) => console.Output.Write(value);

        /// <summary>
        /// Writes an empty line to the console's output stream.
        /// </summary>
        public void WriteLine() => console.Output.WriteLine();

        /// <summary>
        /// Writes a string to the console's output stream, followed by a line terminator.
        /// </summary>
        public void WriteLine(string? value) => console.Output.WriteLine(value);

        /// <summary>
        /// Writes an object to the console's output stream, followed by a line terminator.
        /// </summary>
        public void WriteLine(object? value) => console.Output.WriteLine(value);

        /// <summary>
        /// Reads a single character from the console's input stream.
        /// </summary>
        public int Read() => console.Input.Read();

        /// <summary>
        /// Reads a line from the console's input stream.
        /// </summary>
        public string? ReadLine() => console.Input.ReadLine();

        /// <summary>
        /// Sets the specified foreground color and returns an <see cref="IDisposable" />
        /// that will reset the color back to its previous value upon disposal.
        /// </summary>
        public IDisposable WithForegroundColor(ConsoleColor foregroundColor)
        {
            var lastColor = console.ForegroundColor;
            console.ForegroundColor = foregroundColor;

            return Disposable.Create(() => console.ForegroundColor = lastColor);
        }

        /// <summary>
        /// Sets the specified background color and returns an <see cref="IDisposable" />
        /// that will reset the color back to its previous value upon disposal.
        /// </summary>
        public IDisposable WithBackgroundColor(ConsoleColor backgroundColor)
        {
            var lastColor = console.BackgroundColor;
            console.BackgroundColor = backgroundColor;

            return Disposable.Create(() => console.BackgroundColor = lastColor);
        }

        /// <summary>
        /// Sets the specified foreground and background colors and returns an <see cref="IDisposable" />
        /// that will reset the colors back to their previous values upon disposal.
        /// </summary>
        public IDisposable WithColors(ConsoleColor foregroundColor, ConsoleColor backgroundColor) =>
            Disposable.Merge(
                console.WithForegroundColor(foregroundColor),
                console.WithBackgroundColor(backgroundColor)
            );
    }
}
