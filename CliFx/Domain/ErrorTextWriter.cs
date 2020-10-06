using System;
using System.Collections.Generic;
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

        private static readonly Regex MethodMatcher = new Regex(@"(?<prefix>\S+) (?<name>.*?)(?<methodName>[^\.]+)\(");
        private static readonly Regex ParameterMatcher = new Regex(@"(?<type>.+? )(?<name>.+?)(?:(?<separator>, )|\))");
        private static readonly Regex FileMatcher = new Regex(@"(?<prefix>\S+?) (?<path>.*?)(?<file>[^\\/]+?(?:\.\w*)?):[^:]+? (?<line>\d+).*");

        private readonly IConsole _console;

        public ErrorTextWriter(IConsole console)
        {
            _console = console;
        }

        public void WriteError(Exception ex) => WriteError(ex, 0);
        private void WriteError(Exception ex, int indentLevel)
        {
            var indentation = new string(' ', indent * indentLevel);
            var extraIndentation = new string(' ', indent / 2);

            var exType = ex.GetType();

            // (Fully qualified) type of the exception
            Write(NameColor, indentation + exType.Namespace + ".");
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

            // Print with formatting when successfully parsing all entries in the stack trace
            if (ParseStackTrace(ex.StackTrace) is IEnumerable<StackTraceEntry> parsedStackTrace)
            {
                // Each step in the stack trace is formated and printed
                foreach (var entry in parsedStackTrace)
                {
                    _console.Error.Write(indentation + extraIndentation);

                    WriteMethodDescriptor(entry.MethodPrefix, entry.MethodName, entry.MethodSpecificName);

                    WriteParameters(entry.Parameters);

                    _console.Error.Write(entry.FilePrefix);
                    _console.Error.Write("\n" + indentation + extraIndentation + extraIndentation);
                    WriteFileDescriptor(entry.FilePath, entry.FileName, entry.FileLine);

                    _console.Error.WriteLine();
                }
            }
            else
            {
                // Parsing failed. Print without formatting.
                foreach (var trace in ex.StackTrace.Split('\n'))
                {
                    _console.Error.WriteLine(indentation + trace);
                }
            }
        }

        private void WriteMethodDescriptor(string prefix, string name, string methodName)
        {
            _console.Error.Write(prefix + " ");
            Write(NameColor, name);
            Write(MethodColor, methodName);
        }

        private void WriteParameters(IEnumerable<ParameterEntry> parameters)
        {
            _console.Error.Write("(");
            foreach (var parameter in parameters)
            {
                Write(ParameterTypeColor, parameter.Type);
                Write(SpecificNameColor, parameter.Name);

                if (parameter.Separator is string separator)
                {
                    _console.Error.Write(separator);
                }
            }
            _console.Error.Write(") ");
        }

        private void WriteFileDescriptor(string path, string fileName, string lineNumber)
        {
            Write(NameColor, path);

            Write(FileColor, fileName);
            _console.Error.Write(":");
            Write(LineColor, lineNumber);
        }

        private void Write(ConsoleColor color, string value)
        {
            _console.WithForegroundColor(color, () => _console.Error.Write(value));
        }

        private IEnumerable<StackTraceEntry>? ParseStackTrace(string stackTrace)
        {
            IList<StackTraceEntry> stackTraceEntries = new List<StackTraceEntry>();
            foreach (var trace in stackTrace.Split('\n'))
            {
                var methodMatch = MethodMatcher.Match(trace);
                var parameterMatches = ParameterMatcher.Matches(trace, methodMatch.Index + methodMatch.Length);
                var fileMatch = FileMatcher.Match(
                    trace,
                    parameterMatches.Count switch
                    {
                        0 => methodMatch.Index + methodMatch.Length + 1,
                        int c => parameterMatches[c - 1].Index + parameterMatches[c - 1].Length
                    }
                );

                if (fileMatch.Index + fileMatch.Length != trace.Length)
                {
                    // Didnt match the whole trace
                    return null;
                }

                try
                {
                    IList<ParameterEntry> parameters = new List<ParameterEntry>();
                    foreach (Match match in parameterMatches)
                    {
                        parameters.Add(new ParameterEntry(
                            match.Groups["type"].Success ? match.Groups["type"].Value : throw new Exception("type"),
                            match.Groups["name"].Success ? match.Groups["name"].Value : throw new Exception("name"),
                            match.Groups["separator"]?.Value // If this is null, it's just the last parameter
                        ));
                    }

                    stackTraceEntries.Add(new StackTraceEntry(
                        methodMatch.Groups["prefix"].Success ? methodMatch.Groups["prefix"].Value : throw new Exception("prefix"),
                        methodMatch.Groups["name"].Success ? methodMatch.Groups["name"].Value : throw new Exception("name"),
                        methodMatch.Groups["methodName"].Success ? methodMatch.Groups["methodName"].Value : throw new Exception("methodName"),
                        parameters,
                        fileMatch.Groups["prefix"].Success ? fileMatch.Groups["prefix"].Value : throw new Exception("prefix"),
                        fileMatch.Groups["path"].Success ? fileMatch.Groups["path"].Value : throw new Exception("path"),
                        fileMatch.Groups["file"].Success ? fileMatch.Groups["file"].Value : throw new Exception("file"),
                        fileMatch.Groups["line"].Success ? fileMatch.Groups["line"].Value : throw new Exception("line")
                    ));
                }
                catch
                {
                    // One of the required groups failed to match
                    return null;
                }
            }

            return stackTraceEntries;
        }

        private readonly struct StackTraceEntry
        {
            public string MethodPrefix { get; }
            public string MethodName { get; }
            public string MethodSpecificName { get; }

            public IEnumerable<ParameterEntry> Parameters { get; }

            public string FilePrefix { get; }
            public string FilePath { get; }
            public string FileName { get; }
            public string FileLine { get; }

            public StackTraceEntry(
                string methodPrefix,
                string methodName,
                string methodSpecificName,
                IEnumerable<ParameterEntry> parameters,
                string filePrefix,
                string filePath,
                string fileName,
                string fileLine)
            {
                MethodPrefix = methodPrefix;
                MethodName = methodName;
                MethodSpecificName = methodSpecificName;
                Parameters = parameters;
                FilePrefix = filePrefix;
                FilePath = filePath;
                FileName = fileName;
                FileLine = fileLine;
            }
        }

        private readonly struct ParameterEntry
        {
            public string Type { get; }
            public string Name { get; }
            public string? Separator { get; }

            public ParameterEntry(
                string type,
                string name,
                string? separator)
            {
                Type = type;
                Name = name;
                Separator = separator;
            }
        }
    }
}
