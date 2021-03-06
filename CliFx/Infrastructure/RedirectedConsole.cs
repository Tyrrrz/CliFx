using System;
using System.IO;
using System.Threading;

namespace CliFx.Infrastructure
{
    /// <summary>
    /// Implementation of <see cref="IConsole"/> that uses provided standard input, output, and error streams
    /// instead of the ones exposed by the system console.
    /// This implementation is designed for use in tests.
    /// </summary>
    public class RedirectedConsole : IConsole
    {
        private readonly CancellationToken _cancellationToken;

        /// <inheritdoc />
        public StreamReader Input { get; }

        /// <inheritdoc />
        public bool IsInputRedirected { get; }

        /// <inheritdoc />
        public StreamWriter Output { get; }

        /// <inheritdoc />
        public bool IsOutputRedirected { get; }

        /// <inheritdoc />
        public StreamWriter Error { get; }

        /// <inheritdoc />
        public bool IsErrorRedirected { get; }

        /// <inheritdoc />
        public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Gray;

        /// <inheritdoc />
        public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;

        /// <inheritdoc />
        public void ResetColor()
        {
            ForegroundColor = ConsoleColor.Gray;
            BackgroundColor = ConsoleColor.Black;
        }

        /// <inheritdoc />
        public int CursorLeft { get; set; }

        /// <inheritdoc />
        public int CursorTop { get; set; }

        /// <inheritdoc />
        public CancellationToken RegisterCancellation() => _cancellationToken;

        /// <summary>
        /// Initializes an instance of <see cref="RedirectedConsole"/>.
        /// Use named parameters to specify the streams you want to override.
        /// </summary>
        public RedirectedConsole(
            StreamReader? input = null, bool isInputRedirected = true,
            StreamWriter? output = null, bool isOutputRedirected = true,
            StreamWriter? error = null, bool isErrorRedirected = true,
            CancellationToken cancellationToken = default)
        {
            Input = input ?? StreamReader.Null;
            IsInputRedirected = isInputRedirected;
            Output = output ?? StreamWriter.Null;
            IsOutputRedirected = isOutputRedirected;
            Error = error ?? StreamWriter.Null;
            IsErrorRedirected = isErrorRedirected;
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Initializes an instance of <see cref="RedirectedConsole"/>.
        /// Use named parameters to specify the streams you want to override.
        /// </summary>
        public RedirectedConsole(
            Stream? input = null, bool isInputRedirected = true,
            Stream? output = null, bool isOutputRedirected = true,
            Stream? error = null, bool isErrorRedirected = true,
            CancellationToken cancellationToken = default)
            : this(
                ConsoleStream.WrapInput(input), isInputRedirected,
                ConsoleStream.WrapOutput(output), isOutputRedirected,
                ConsoleStream.WrapOutput(error), isErrorRedirected,
                cancellationToken)
        {
        }
    }
}