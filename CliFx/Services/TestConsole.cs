using System;
using System.IO;

namespace CliFx.Services
{
    public class TestConsole : IConsole
    {
        public TextReader Input { get; }

        public bool IsInputRedirected => true;

        public TextWriter Output { get; }

        public bool IsOutputRedirected => true;

        public TextWriter Error { get; }

        public bool IsErrorRedirected => true;

        public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Gray;

        public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;

        public TestConsole(TextReader input, TextWriter output, TextWriter error)
        {
            Input = input;
            Output = output;
            Error = error;
        }

        public TestConsole(TextWriter output, TextWriter error)
            : this(TextReader.Null, output, error)
        {
        }

        public TestConsole(TextWriter output)
            : this(output, TextWriter.Null)
        {
        }

        public void ResetColor()
        {
            ForegroundColor = ConsoleColor.Gray;
            BackgroundColor = ConsoleColor.Black;
        }
    }
}