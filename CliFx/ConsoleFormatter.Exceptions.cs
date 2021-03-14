using System;
using System.IO;
using System.Linq;
using CliFx.Utils;

namespace CliFx
{
    internal partial class ConsoleFormatter
    {
        private void WriteException(Exception exception, int indentLevel)
        {
            var exceptionType = exception.GetType();

            WriteHorizontalMargin(4 * indentLevel);

            // Fully qualified exception type
            Write(ConsoleColor.DarkGray, exceptionType.Namespace + '.');
            Write(ConsoleColor.White, exceptionType.Name);
            Write(": ");

            // Exception message
            Write(ConsoleColor.Red, exception.Message);
            WriteLine();

            // Recurse into inner exceptions
            if (exception.InnerException is not null)
            {
                WriteException(exception.InnerException, indentLevel + 1);
            }

            // Try to parse and pretty-print the stack trace
            var stackFrames = !string.IsNullOrWhiteSpace(exception.StackTrace)
                ? StackFrame.ParseMany(exception.StackTrace).ToArray()
                : Array.Empty<StackFrame>();

            foreach (var stackFrame in stackFrames)
            {
                WriteHorizontalMargin(2 + 4 * indentLevel);
                Write("at ");

                // Fully qualified method name
                Write(ConsoleColor.DarkGray, stackFrame.ParentType + '.');
                Write(ConsoleColor.Yellow, stackFrame.MethodName);

                // Method parameters

                Write('(');

                for (var i = 0; i < stackFrame.Parameters.Count; i++)
                {
                    var parameter = stackFrame.Parameters[i];

                    // Separator
                    if (i > 0)
                    {
                        Write(", ");
                    }

                    // Parameter type
                    Write(ConsoleColor.Blue, parameter.Type);

                    // Parameter name (can be null for dynamically generated methods)
                    if (!string.IsNullOrWhiteSpace(parameter.Name))
                    {
                        Write(' ');
                        Write(ConsoleColor.White, parameter.Name);
                    }
                }

                Write(") ");

                // Location
                if (!string.IsNullOrWhiteSpace(stackFrame.FilePath))
                {
                    var stackFrameDirectoryPath =
                        Path.GetDirectoryName(stackFrame.FilePath) + Path.DirectorySeparatorChar;

                    var stackFrameFileName = Path.GetFileName(stackFrame.FilePath);

                    Write("in ");

                    // File path
                    Write(ConsoleColor.DarkGray, stackFrameDirectoryPath);
                    Write(ConsoleColor.Yellow, stackFrameFileName);

                    // Source position
                    if (!string.IsNullOrWhiteSpace(stackFrame.LineNumber))
                    {
                        Write(':');
                        Write(ConsoleColor.Blue, stackFrame.LineNumber);
                    }
                }

                WriteLine();
            }
        }

        public void WriteException(Exception exception)
        {
            Write(ConsoleColor.Red, ConsoleColor.DarkRed, "ERROR");
            WriteLine();
            WriteException(exception, 0);
        }
    }
}