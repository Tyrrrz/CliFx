using System;
using System.IO;

namespace CliFx.Services
{
    public interface IConsole
    {
        TextReader Input { get; }

        bool IsInputRedirected { get; }

        TextWriter Output { get; }

        bool IsOutputRedirected { get; }

        TextWriter Error { get; }

        bool IsErrorRedirected { get; }

        ConsoleColor ForegroundColor { get; set; }

        ConsoleColor BackgroundColor { get; set; }

        void ResetColor();
    }
}