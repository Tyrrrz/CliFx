using System;
using System.IO;

namespace CliFx.Services
{
    /// <summary>
    /// Implementation of <see cref="IConsole"/> that wraps around <see cref="Console"/>.
    /// </summary>
    public class SystemConsole : IConsole
    {
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
    }
}