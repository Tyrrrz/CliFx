using System;
using System.IO;

namespace CliFx.Services
{
    public class SystemConsole : IConsole
    {
        public TextReader Input => Console.In;

        public bool IsInputRedirected => Console.IsInputRedirected;

        public TextWriter Output => Console.Out;

        public bool IsOutputRedirected => Console.IsOutputRedirected;

        public TextWriter Error => Console.Error;

        public bool IsErrorRedirected => Console.IsErrorRedirected;

        public ConsoleColor ForegroundColor
        {
            get => Console.ForegroundColor;
            set => Console.ForegroundColor = value;
        }

        public ConsoleColor BackgroundColor
        {
            get => Console.BackgroundColor;
            set => Console.BackgroundColor = value;
        }

        public void ResetColor() => Console.ResetColor();
    }
}