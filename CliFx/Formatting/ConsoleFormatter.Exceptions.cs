using System;
using System.IO;
using System.Linq;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using CliFx.Utils;

namespace CliFx.Formatting
{
    internal partial class ConsoleFormatter
    {
        private static void WriteException(ConsoleFormatter formatter, Exception exception, int indentLevel)
        {
            formatter.WriteHorizontalMargin(4 * indentLevel);

            // Fully qualified exception type
            var exceptionType = exception.GetType();
            formatter.Write(ConsoleColor.DarkGray, exceptionType.Namespace + '.');
            formatter.Write(ConsoleColor.White, exceptionType.Name);
            formatter.Write(": ");

            // Exception message
            formatter.Write(ConsoleColor.Red, exception.Message);
            formatter.WriteLine();

            // Recurse into inner exceptions
            if (exception.InnerException is not null)
            {
                WriteException(formatter, exception.InnerException, indentLevel + 1);
            }

            // Try to parse and pretty-print the stack trace
            var stackFrames = !string.IsNullOrWhiteSpace(exception.StackTrace)
                ? StackFrame.ParseMany(exception.StackTrace).ToArray()
                : Array.Empty<StackFrame>();

            foreach (var stackFrame in stackFrames)
            {
                formatter.WriteHorizontalMargin(2 + 4 * indentLevel);
                formatter.Write("at ");

                // Fully qualified method name
                formatter.Write(ConsoleColor.DarkGray, stackFrame.ParentType + '.');
                formatter.Write(ConsoleColor.Yellow, stackFrame.MethodName);

                // Method parameters

                formatter.Write('(');

                for (var i = 0; i < stackFrame.Parameters.Count; i++)
                {
                    var parameter = stackFrame.Parameters[i];

                    // Separator
                    if (i > 0)
                    {
                        formatter.Write(", ");
                    }

                    // Parameter type
                    formatter.Write(ConsoleColor.Blue, parameter.Type);

                    // Parameter name (can be null for dynamically generated methods)
                    if (!string.IsNullOrWhiteSpace(parameter.Name))
                    {
                        formatter.Write(' ');
                        formatter.Write(ConsoleColor.White, parameter.Name);
                    }
                }

                formatter.Write(") ");

                // Location
                if (!string.IsNullOrWhiteSpace(stackFrame.FilePath))
                {
                    var stackFrameDirectoryPath =
                        Path.GetDirectoryName(stackFrame.FilePath) + Path.DirectorySeparatorChar;

                    var stackFrameFileName = Path.GetFileName(stackFrame.FilePath);

                    formatter.Write("in ");

                    // File path
                    formatter.Write(ConsoleColor.DarkGray, stackFrameDirectoryPath);
                    formatter.Write(ConsoleColor.Yellow, stackFrameFileName);

                    // Source position
                    if (!string.IsNullOrWhiteSpace(stackFrame.LineNumber))
                    {
                        formatter.Write(':');
                        formatter.Write(ConsoleColor.Blue, stackFrame.LineNumber);
                    }
                }

                formatter.WriteLine();
            }
        }

        public static void WriteException(IConsole console, Exception exception)
        {
            var formatter = new ConsoleFormatter(console, true);

            // Domain exceptions should be printed with minimal information
            // because they are meant for the user of the application,
            // not the user of the library.
            if (exception is CliFxException cliFxException &&
                !string.IsNullOrWhiteSpace(cliFxException.ActualMessage))
            {
                formatter.Write(ConsoleColor.Red, cliFxException.ActualMessage);
            }
            // All other exceptions most likely indicate an actual bug
            // and should include stacktrace and other detailed information.
            else
            {
                formatter.Write(ConsoleColor.White, ConsoleColor.DarkRed, "ERROR");
                formatter.WriteLine();
                WriteException(formatter, exception, 0);
            }
        }
    }
}