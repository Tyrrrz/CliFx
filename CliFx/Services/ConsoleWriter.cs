using System;
using System.IO;
using CliFx.Models;

namespace CliFx.Services
{
    public partial class ConsoleWriter : IConsoleWriter, IDisposable
    {
        private readonly TextWriter _textWriter;
        private readonly bool _isRedirected;

        public ConsoleWriter(TextWriter textWriter, bool isRedirected)
        {
            _textWriter = textWriter;
            _isRedirected = isRedirected;
        }

        // TODO: handle colors
        public void Write(TextSpan text) => _textWriter.Write(text.Text);

        public void WriteLine(TextSpan text) => _textWriter.WriteLine(text.Text);

        public void Dispose() => _textWriter.Dispose();
    }

    public partial class ConsoleWriter
    {
        public static ConsoleWriter GetStandardOutput() => new ConsoleWriter(Console.Out, Console.IsOutputRedirected);

        public static ConsoleWriter GetStandardError() => new ConsoleWriter(Console.Error, Console.IsErrorRedirected);
    }
}