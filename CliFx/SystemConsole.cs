using System;
using System.IO;
using System.Threading;

namespace CliFx
{
    /// <summary>
    /// Implementation of <see cref="IConsole"/> that wraps the default system console.
    /// </summary>
    public class SystemConsole : IConsole
    {
        private CancellationTokenSource? _cancellationTokenSource;

        /// <inheritdoc />
        public TextReader Input => Console.In;

        /// <inheritdoc />
        public bool IsInputRedirected => Console.IsInputRedirected;

        /// <inheritdoc />
        public TextWriter Output => Console.Out;

        /// <inheritdoc />
        public bool IsOutputRedirected => Console.IsOutputRedirected;

        /// <inheritdoc />
        public TextWriter Error => Console.Error;

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
}