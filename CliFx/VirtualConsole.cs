using System;
using System.IO;
using System.Threading;

namespace CliFx
{
    /// <summary>
    /// Implementation of <see cref="IConsole"/> that routes data to specified streams.
    /// Does not leak to system console in any way.
    /// Use this class as a substitute for system console when running tests.
    /// </summary>
    public class VirtualConsole : IConsole
    {
        private readonly CancellationToken _cancellationToken;

        /// <inheritdoc />
        public TextReader Input { get; }

        /// <inheritdoc />
        public bool IsInputRedirected { get; }

        /// <inheritdoc />
        public TextWriter Output { get; }

        /// <inheritdoc />
        public bool IsOutputRedirected { get; }

        /// <inheritdoc />
        public TextWriter Error { get; }

        /// <inheritdoc />
        public bool IsErrorRedirected { get; }

        /// <inheritdoc />
        public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Gray;

        /// <inheritdoc />
        public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;

        /// <summary>
        /// Initializes an instance of <see cref="VirtualConsole"/>.
        /// </summary>
        public VirtualConsole(TextReader input, bool isInputRedirected,
            TextWriter output, bool isOutputRedirected,
            TextWriter error, bool isErrorRedirected,
            CancellationToken cancellationToken = default)
        {
            Input = input;
            IsInputRedirected = isInputRedirected;
            Output = output;
            IsOutputRedirected = isOutputRedirected;
            Error = error;
            IsErrorRedirected = isErrorRedirected;
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Initializes an instance of <see cref="VirtualConsole"/>.
        /// </summary>
        public VirtualConsole(TextReader input, TextWriter output, TextWriter error,
            CancellationToken cancellationToken = default)
            : this(input, true, output, true, error, true, cancellationToken)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="VirtualConsole"/> using output stream (stdout) and error stream (stderr).
        /// Input stream (stdin) is replaced with a no-op stub.
        /// </summary>
        public VirtualConsole(TextWriter output, TextWriter error, CancellationToken cancellationToken = default)
            : this(TextReader.Null, output, error, cancellationToken)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="VirtualConsole"/> using output stream (stdout).
        /// Input stream (stdin) and error stream (stderr) are replaced with no-op stubs.
        /// </summary>
        public VirtualConsole(TextWriter output, CancellationToken cancellationToken = default)
            : this(output, TextWriter.Null, cancellationToken)
        {
        }

        /// <inheritdoc />
        public void ResetColor()
        {
            ForegroundColor = ConsoleColor.Gray;
            BackgroundColor = ConsoleColor.Black;
        }

        /// <inheritdoc />
        public CancellationToken GetCancellationToken() => _cancellationToken;
    }
}