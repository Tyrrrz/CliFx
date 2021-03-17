using System;
using CliFx.Infrastructure;

namespace CliFx.Formatting
{
    internal class ConsoleFormatter
    {
        private readonly ConsoleWriter _consoleWriter;

        private int _column;
        private int _row;

        public bool IsEmpty => _column == 0 && _row == 0;

        public ConsoleFormatter(ConsoleWriter consoleWriter) =>
            _consoleWriter = consoleWriter;

        public void Write(string value)
        {
            _consoleWriter.Write(value);
            _column += value.Length;
        }

        public void Write(char value)
        {
            _consoleWriter.Write(value);
            _column++;
        }

        public void Write(ConsoleColor foregroundColor, string value)
        {
            using (_consoleWriter.Console.WithForegroundColor(foregroundColor))
                Write(value);
        }

        public void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            using (_consoleWriter.Console.WithColors(foregroundColor, backgroundColor))
                Write(value);
        }

        public void WriteLine()
        {
            _consoleWriter.WriteLine();
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
    }
}