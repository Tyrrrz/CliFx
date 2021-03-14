using System;
using System.Collections.Generic;
using System.IO;
using CliFx.Infrastructure;
using CliFx.Input;
using CliFx.Schema;

namespace CliFx
{
    internal partial class ConsoleFormatter
    {
        private readonly IConsole _console;
        private readonly bool _isError;

        private int _column;
        private int _row;

        private TextWriter Writer => !_isError ? _console.Output : _console.Error;

        private bool IsEmpty => _column == 0 && _row == 0;

        public ConsoleFormatter(IConsole console, bool isError)
        {
            _console = console;
            _isError = isError;
        }

        private void Write(string value)
        {
            Writer.Write(value);
            _column += value.Length;
        }

        private void Write(char value)
        {
            Writer.Write(value);
            _column++;
        }

        private void Write(ConsoleColor foregroundColor, string value)
        {
            using (_console.WithForegroundColor(foregroundColor))
                Write(value);
        }

        private void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            using (_console.WithColors(foregroundColor, backgroundColor))
                Write(value);
        }

        private void WriteLine()
        {
            Writer.WriteLine();
            _column = 0;
            _row++;
        }

        private void WriteVerticalMargin(int size = 1)
        {
            for (var i = 0; i < size; i++)
                WriteLine();
        }

        private void WriteHorizontalMargin(int size = 2)
        {
            for (var i = 0; i < size; i++)
                Write(' ');
        }

        private void WriteColumnMargin(int columnSize = 20, int offsetSize = 2)
        {
            if (_column + offsetSize < columnSize)
                WriteHorizontalMargin(columnSize - _column);
            else
                WriteHorizontalMargin(offsetSize);
        }

        private void WriteHeader(string text)
        {
            Write(ConsoleColor.Magenta, text);
            WriteLine();
        }
    }

    internal static class ConsoleFormatterExtensions
    {
        public static void WriteException(
            this IConsole console,
            Exception exception)
        {
            var formatter = new ConsoleFormatter(console, true);
            formatter.WriteException(exception);
        }

        public static void WriteCommandInput(
            this IConsole console,
            CommandInput input)
        {
            var formatter = new ConsoleFormatter(console, false);
            formatter.WriteCommandInput(input);
        }

        public static void WriteHelpText(
            this IConsole console,
            ApplicationMetadata applicationMetadata,
            ApplicationSchema applicationSchema,
            CommandSchema commandSchema,
            IReadOnlyDictionary<MemberSchema, object?> defaultValues)
        {
            var formatter = new ConsoleFormatter(console, false);
            formatter.WriteHelpText(applicationMetadata, applicationSchema, commandSchema, defaultValues);
        }
    }
}