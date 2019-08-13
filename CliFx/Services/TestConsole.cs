using System;
using System.IO;

namespace CliFx.Services
{
    /// <summary>
    /// Implementation of <see cref="IConsole"/> that routes data to specified streams.
    /// Does not leak to <see cref="Console"/> in any way.
    /// Provides an isolated instance of <see cref="IConsole"/> which is useful for testing purposes.
    /// </summary>
    public class TestConsole : IConsole
    {
        /// <inheritdoc />
        public TextReader Input { get; }

        /// <inheritdoc />
        public bool IsInputRedirected => true;

        /// <inheritdoc />
        public TextWriter Output { get; }

        /// <inheritdoc />
        public bool IsOutputRedirected => true;

        /// <inheritdoc />
        public TextWriter Error { get; }

        /// <inheritdoc />
        public bool IsErrorRedirected => true;

        /// <inheritdoc />
        public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Gray;

        /// <inheritdoc />
        public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;

        /// <summary>
        /// Initializes an instance of <see cref="TestConsole"/>.
        /// </summary>
        public TestConsole(TextReader input, TextWriter output, TextWriter error)
        {
            Input = input;
            Output = output;
            Error = error;
        }

        /// <summary>
        /// Initializes an instance of <see cref="TestConsole"/> using output stream (stdout) and error stream (stderr).
        /// Input stream (stdin) is considered empty.
        /// </summary>
        public TestConsole(TextWriter output, TextWriter error)
            : this(TextReader.Null, output, error)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="TestConsole"/> using output stream (stdout).
        /// Input stream (stdin) and error stream (stderr) are considered empty.
        /// </summary>
        public TestConsole(TextWriter output)
            : this(output, TextWriter.Null)
        {
        }

        /// <inheritdoc />
        public void ResetColor()
        {
            ForegroundColor = ConsoleColor.Gray;
            BackgroundColor = ConsoleColor.Black;
        }
    }
}