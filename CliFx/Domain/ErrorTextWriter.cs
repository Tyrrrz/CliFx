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

        private readonly IConsole _console;
        public ErrorTextWriter(IConsole console)
        {
            _console = console;
        }

        public void WriteError(Exception ex) => WriteError(ex, 0);
        private void WriteError(Exception ex, int indentLevel)
        {
            var indentation = new String(' ', indent * indentLevel);
            var extraIndentation = new String(' ', indent / 2);

            var exType = ex.GetType();

            Write(NameColor, indentation + exType.Namespace + ".");
            Write(SpecificNameColor, exType.Name);
            _console.Error.Write(": ");
            Write(MessageColor, ex.Message);
            _console.Error.WriteLine();

            if (ex.InnerException is Exception innerException)
			{
                WriteError(innerException, indentLevel + 1);
			}

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

                _console.Error.Write(indentation + extraIndentation + methodMatch.Groups["prefix"].Value + " ");
                Write(NameColor, methodMatch.Groups["name"].Value);
                Write(MethodColor, methodMatch.Groups["methodName"].Value);

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
                _console.Error.WriteLine(") " + fileMatch.Groups["prefix"].Value);

                Write(NameColor, indentation + extraIndentation + extraIndentation + fileMatch.Groups["path"].Value);
                Write(FileColor, fileMatch.Groups["file"].Value);
                _console.Error.Write(":");
                Write(LineColor, fileMatch.Groups["line"].Value);
                _console.Error.WriteLine();
            }
        }

        private void Write(ConsoleColor color, string value)
        {
            _console.WithForegroundColor(color, () => _console.Error.Write(value));
        }
    }
}
