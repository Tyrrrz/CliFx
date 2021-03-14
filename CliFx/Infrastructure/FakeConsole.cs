using System;
using System.IO;
using System.Threading;

namespace CliFx.Infrastructure
{
    /// <summary>
    /// Implementation of <see cref="IConsole"/> that uses the provided
    /// fake standard input, output, and error streams.
    /// </summary>
    /// <remarks>
    /// Use this implementation in tests to verify how a command interacts with the console.
    /// </remarks>
    public class FakeConsole : IConsole, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        /// <inheritdoc />
        public StreamReader Input { get; }

        /// <inheritdoc />
        public bool IsInputRedirected => true;

        /// <inheritdoc />
        public StreamWriter Output { get; }

        /// <inheritdoc />
        public bool IsOutputRedirected => true;

        /// <inheritdoc />
        public StreamWriter Error { get; }

        /// <inheritdoc />
        public bool IsErrorRedirected => true;

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

        /// <summary>
        /// Initializes an instance of <see cref="FakeConsole"/>.
        /// </summary>
        public FakeConsole(StreamReader? input = null, StreamWriter? output = null, StreamWriter? error = null)
        {
            Input = input ?? StreamReader.Null;
            Output = output ?? StreamWriter.Null;
            Error = error ?? StreamWriter.Null;
        }

        /// <summary>
        /// Initializes an instance of <see cref="FakeConsole"/>.
        /// </summary>
        public FakeConsole(Stream? input = null, Stream? output = null, Stream? error = null)
            : this(ConsoleStream.WrapInput(input), ConsoleStream.WrapOutput(output), ConsoleStream.WrapOutput(error))
        {
        }

        /// <inheritdoc />
        public CancellationToken RegisterCancellation() => _cancellationTokenSource.Token;

        /// <summary>
        /// Sends a cancellation signal to the currently executing command.
        /// </summary>
        /// <remarks>
        /// If the command is not cancellation-aware (i.e. it doesn't call <see cref="IConsole.RegisterCancellation"/>),
        /// this method will not have any effect.
        /// </remarks>
        public void RequestCancellation(TimeSpan delay)
        {
            // Avoid unnecessary creation of a timer
            if (delay > TimeSpan.Zero)
            {
                _cancellationTokenSource.CancelAfter(delay);
            }
            else
            {
                _cancellationTokenSource.Cancel();
            }
        }

        /// <inheritdoc cref="RequestCancellation(System.TimeSpan)" />
        public void RequestCancellation() => RequestCancellation(TimeSpan.Zero);

        /// <inheritdoc />
        public virtual void Dispose() => _cancellationTokenSource.Dispose();
    }
}