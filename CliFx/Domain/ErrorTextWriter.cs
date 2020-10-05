using System;
using System.Text.RegularExpressions;

namespace CliFx.Domain
{
    internal class ErrorTextWriter
    {
        private const int indent = 4;

        private static readonly ConsoleColor NameColor = ConsoleColor.DarkGray;
        private static readonly ConsoleColor SpecificNameColor = ConsoleColor.White;
        private static readonly ConsoleColor MessageColor = ConsoleColor.Red;
        private static readonly ConsoleColor MethodColor = ConsoleColor.Yellow;
        private static readonly ConsoleColor ParameterTypeColor = ConsoleColor.Blue;
        private static readonly ConsoleColor FileColor = ConsoleColor.Yellow;
        private static readonly ConsoleColor LineColor = ConsoleColor.Blue;

        private static readonly Lazy<Regex> MethodMatcher = new Lazy<Regex>(() => new Regex(@"(?<prefix>\S+) (?<name>.*?)(?<methodName>[^\.]+)\("));
        private static readonly Lazy<Regex> ParameterMatcher = new Lazy<Regex>(() => new Regex(@"(?<type>.+? )(?<name>.+?)(?:(?<separator>, )|\))"));
        private static readonly Lazy<Regex> FileMatcher = new Lazy<Regex>(() => new Regex(@"(?<prefix>\S+?) (?<path>.*?)(?<file>[^\\/]+?(?:\.\w*)?):[^:]+? (?<line>\d+)"));

        private readonly ApplicationConfiguration _configuration;
        private readonly IConsole _console;

        public ErrorTextWriter(ApplicationConfiguration configuration, IConsole console)
        {
            _configuration = configuration;
            _console = console;
        }

        public void WriteError(Exception ex) => WriteError(ex, 0);
        private void WriteError(Exception ex, int indentLevel)
        {
            var indentation = new String(' ', indent * indentLevel);
            var extraIndentation = new String(' ', indent / 2);

            var exType = ex.GetType();

            _console.Error.Write(indentation);

            // (Fully qualified) type of the exception
            if (!_configuration.IsUsingShortErrors)
            {
                Write(NameColor, exType.Namespace + ".");
            }
            Write(SpecificNameColor, exType.Name);
            _console.Error.Write(": ");

            // Message
            Write(MessageColor, ex.Message);
            _console.Error.WriteLine();

            // Prints the inner exception
            // with one higher indentation level
            if (ex.InnerException is Exception innerException)
            {
                WriteError(innerException, indentLevel + 1);
            }

            // Each step in the stack trace is formated and printed
            foreach (var trace in ex.StackTrace.Split('\n'))
            {
                var methodMatch = MethodMatcher.Value.Match(trace);
                var parameterMatches = ParameterMatcher.Value.Matches(trace, methodMatch.Index + methodMatch.Length);
                var fileMatch = FileMatcher.Value.Match(
                    trace,
                    parameterMatches.Count switch
                    {
                        0 => methodMatch.Index + methodMatch.Length + 1,
                        int c => parameterMatches[c - 1].Index + parameterMatches[c - 1].Length
                    }
                );

                _console.Error.Write(indentation + extraIndentation);

                WriteMethodDescriptor(methodMatch.Groups["prefix"].Value, methodMatch.Groups["name"].Value, methodMatch.Groups["methodName"].Value);

                WriteParameters(parameterMatches);

                _console.Error.Write(fileMatch.Groups["prefix"].Value);
                if (!_configuration.IsUsingShortErrors)
                {
                    _console.Error.Write("\n" + indentation + extraIndentation + extraIndentation);
                }
                else
                {
                    _console.Error.Write(" ");
                }
                WriteFileDescriptor(fileMatch.Groups["path"].Value, fileMatch.Groups["file"].Value, fileMatch.Groups["line"].Value);

                _console.Error.WriteLine();
            }
        }

        private void WriteMethodDescriptor(string prefix, string name, string methodName)
        {
            _console.Error.Write(prefix + " ");
            if (!_configuration.IsUsingShortErrors)
            {
                Write(NameColor, name);
            }
            Write(MethodColor, methodName);
        }

        private void WriteParameters(MatchCollection parameterMatches)
        {
            _console.Error.Write("(");
            foreach (Match parameterMatch in parameterMatches)
            {
                Write(ParameterTypeColor, parameterMatch.Groups["type"].Value);
                Write(SpecificNameColor, parameterMatch.Groups["name"].Value);

                if (parameterMatch.Groups["separator"] is Group separatorGroup)
                {
                    _console.Error.Write(separatorGroup.Value);
                }
            }
            _console.Error.Write(") ");
        }

        private void WriteFileDescriptor(string path, string fileName, string lineNumber)
        {
            if (!_configuration.IsUsingShortErrors)
            {
                Write(NameColor, path);
            }

            Write(FileColor, fileName);
            _console.Error.Write(":");
            Write(LineColor, lineNumber);
        }

        private void Write(ConsoleColor color, string value)
        {
            _console.WithForegroundColor(color, () => _console.Error.Write(value));
        }
    }
}
