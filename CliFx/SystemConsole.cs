using System;
using System.IO;
using System.Threading;

namespace CliFx
{
    /// <summary>
    /// Implementation of <see cref="IConsole"/> that wraps the default system console.
    /// </summary>
    public partial class SystemConsole : IConsole
    {
        private CancellationTokenSource? _cancellationTokenSource;

        /// <inheritdoc />
        public StreamReader Input { get; }

        /// <inheritdoc />
        public bool IsInputRedirected => Console.IsInputRedirected;

        /// <inheritdoc />
        public StreamWriter Output { get; }

        /// <inheritdoc />
        public bool IsOutputRedirected => Console.IsOutputRedirected;

        /// <inheritdoc />
        public StreamWriter Error { get; }

        /// <inheritdoc />
        public bool IsErrorRedirected => Console.IsErrorRedirected;

        /// <inheritdoc />
        public ConsoleColor ForegroundColor
        {
            get => Console.ForegroundColor;
            set => Console.ForegroundColor = value;
        }

        /// <inheritdoc />
        public ConsoleColor BackgroundColor
        {
            get => Console.BackgroundColor;
            set => Console.BackgroundColor = value;
        }

        /// <summary>
        /// Initializes an instance of <see cref="SystemConsole"/>.
        /// </summary>
        public SystemConsole()
        {
            Input = WrapInput(Console.OpenStandardInput());
            Output = WrapOutput(Console.OpenStandardOutput());
            Error = WrapOutput(Console.OpenStandardError());
        }

        /// <inheritdoc />
        public void ResetColor() => Console.ResetColor();

        /// <inheritdoc />
        public CancellationToken GetCancellationToken()
        {
            if (_cancellationTokenSource != null)
                return _cancellationTokenSource.Token;

            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (_, args) =>
            {
                // If cancellation hasn't been requested yet - cancel shutdown and fire the token
                if (!cts.IsCancellationRequested)
                {
                    args.Cancel = true;
                    cts.Cancel();
                }
            };

            return (_cancellationTokenSource = cts).Token;
        }
    }

    public partial class SystemConsole
    {
        private static StreamReader WrapInput(Stream? stream) =>
            stream != null
                ? new StreamReader(stream, Console.InputEncoding, false)
                : StreamReader.Null;

        private static StreamWriter WrapOutput(Stream? stream) =>
            stream != null
                ? new StreamWriter(stream, Console.OutputEncoding) {AutoFlush = true}
                : StreamWriter.Null;
    }
}