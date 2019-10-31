using System;
using System.IO;
using System.Threading;

namespace CliFx.Services
{
    /// <summary>
    /// Implementation of <see cref="IConsole"/> that wraps around <see cref="Console"/>.
    /// </summary>
    public class SystemConsole : IConsole
    {
        private CancellationTokenSource _cancellationTokenSource;
        
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
            if (_cancellationTokenSource is null)
            {
                _cancellationTokenSource = new CancellationTokenSource();

                // Subscribe to CancelKeyPress event with cancellation token source
                // Kills app on second cancellation (hard cancellation)
                Console.CancelKeyPress += (_, args) =>
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                        return;
                    args.Cancel = true;
                    _cancellationTokenSource.Cancel();
                };
            }

            return _cancellationTokenSource.Token;
        }
    }
}