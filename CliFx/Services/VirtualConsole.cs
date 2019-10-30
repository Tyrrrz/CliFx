using System;
using System.IO;
using System.Threading;
using CliFx.Internal;

namespace CliFx.Services
{
    /// <summary>
    /// Implementation of <see cref="IConsole"/> that routes data to specified streams.
    /// Does not leak to <see cref="Console"/> in any way.
    /// Provides an isolated instance of <see cref="IConsole"/> which is useful for testing purposes.
    /// </summary>
    public class VirtualConsole : IConsole
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

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
            TextWriter error, bool isErrorRedirected)
        {
            Input = input.GuardNotNull(nameof(input));
            IsInputRedirected = isInputRedirected;
            Output = output.GuardNotNull(nameof(output));
            IsOutputRedirected = isOutputRedirected;
            Error = error.GuardNotNull(nameof(error));
            IsErrorRedirected = isErrorRedirected;
        }

        /// <summary>
        /// Initializes an instance of <see cref="VirtualConsole"/>.
        /// </summary>
        public VirtualConsole(TextReader input, TextWriter output, TextWriter error)
            : this(input, true, output, true, error, true)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="VirtualConsole"/> using output stream (stdout) and error stream (stderr).
        /// Input stream (stdin) is replaced with a no-op stub.
        /// </summary>
        public VirtualConsole(TextWriter output, TextWriter error)
            : this(TextReader.Null, output, error)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="VirtualConsole"/> using output stream (stdout).
        /// Input stream (stdin) and error stream (stderr) are replaced with no-op stubs.
        /// </summary>
        public VirtualConsole(TextWriter output)
            : this(output, TextWriter.Null)
        {
        }

        /// <inheritdoc />
        public void ResetColor()
        {
            ForegroundColor = ConsoleColor.Gray;
            BackgroundColor = ConsoleColor.Black;
        }

        /// <inheritdoc />
        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        /// <summary>
        /// Simulates cancellation.
        /// </summary>
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}