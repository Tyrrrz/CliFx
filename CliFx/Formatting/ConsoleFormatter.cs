using System;
using System.IO;
using CliFx.Infrastructure;

namespace CliFx.Formatting
{
    // TODO: rework
    internal partial class ConsoleFormatter
    {
        private readonly IConsole _console;
        private readonly bool _isError;

        private int _column;
        private int _row;

        private TextWriter Writer => _isError switch
        {
            true => _console.Error,
            _ => _console.Output
        };

        public bool IsEmpty => _column == 0 && _row == 0;

        public ConsoleFormatter(IConsole console, bool isError)
        {
            _console = console;
            _isError = isError;
        }

        public void Write(string value)
        {
            Writer.Write(value);
            _column += value.Length;
        }

        public void Write(char value)
        {
            Writer.Write(value);
            _column++;
        }

        public void Write(ConsoleColor foregroundColor, string value)
        {
            using (_console.WithForegroundColor(foregroundColor))
                Write(value);
        }

        public void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            using (_console.WithColors(foregroundColor, backgroundColor))
                Write(value);
        }

        public void WriteLine()
        {
            Writer.WriteLine();
            _column = 0;
            _row++;
        }

        public void WriteVerticalMargin(int size = 1)
        {
            for (var i = 0; i < size; i++)
                WriteLine();
        }

        public void WriteHorizontalMargin(int size = 2)
        {
            for (var i = 0; i < size; i++)
                Write(' ');
        }

        public void WriteColumnMargin(int columnSize = 20, int offsetSize = 2)
        {
            if (_column + offsetSize < columnSize)
                WriteHorizontalMargin(columnSize - _column);
            else
                WriteHorizontalMargin(offsetSize);
        }

        public void WriteHeader(string text)
        {
            Write(ConsoleColor.Magenta, text);
            WriteLine();
        }
    }
}