using System;
using System.IO;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using CliFx.Utils;
using CliFx.Utils.Extensions;

namespace CliFx.Formatting;

internal class ExceptionConsoleFormatter(ConsoleWriter consoleWriter)
    : ConsoleFormatter(consoleWriter)
{
    private void WriteStackFrame(StackFrame stackFrame, int indentLevel)
    {
        WriteHorizontalMargin(2 + 4 * indentLevel);
        Write("at ");

        // Fully qualified method name
        Write(stackFrame.ParentType + '.');
        Write(ConsoleColor.Yellow, stackFrame.MethodName);

        // Method parameters

        Write('(');

        foreach (var (parameter, i) in stackFrame.Parameters.WithIndex())
        {
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
            Write(stackFrameDirectoryPath);
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

    private void WriteException(Exception exception, int indentLevel)
    {
        WriteHorizontalMargin(4 * indentLevel);

        // Fully qualified exception type
        var exceptionType = exception.GetType();
        Write(exceptionType.Namespace + '.');
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

        // Non-thrown exceptions (e.g. inner exceptions) have no stacktrace
        if (!string.IsNullOrWhiteSpace(exception.StackTrace))
        {
            // Parse and pretty-print the stacktrace
            foreach (var stackFrame in StackFrame.ParseTrace(exception.StackTrace))
                WriteStackFrame(stackFrame, indentLevel);
        }
    }

    public void WriteException(Exception exception)
    {
        // Domain exceptions should be printed with minimal information because they are
        // meant for the user of the application, not the user of the library.
        if (exception is CliFxException { HasCustomMessage: true } cliFxException)
        {
            Write(ConsoleColor.Red, cliFxException.Message);
            WriteLine();
        }
        // All other exceptions most likely indicate an actual bug and should include
        // the stacktrace and other detailed information.
        else
        {
            Write(ConsoleColor.White, ConsoleColor.DarkRed, "ERROR");
            WriteLine();
            WriteException(exception, 0);
        }
    }
}

internal static class ExceptionConsoleFormatterExtensions
{
    public static void WriteException(this ConsoleWriter consoleWriter, Exception exception) =>
        new ExceptionConsoleFormatter(consoleWriter).WriteException(exception);

    public static void WriteException(this IConsole console, Exception exception) =>
        console.Error.WriteException(exception);
}
