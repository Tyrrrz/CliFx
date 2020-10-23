using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CliFx.Internal.Extensions;

namespace CliFx.Internal
{
    internal class StackFrameParameter
    {
        public string Type { get; }

        public string Name { get; }

        public string? Separator { get; }

        public StackFrameParameter(
            string type,
            string name,
            string? separator)
        {
            Type = type;
            Name = name;
            Separator = separator;
        }
    }

    internal partial class StackFrame
    {
        public string Prefix { get; }

        public string ParentType { get; }

        public string MethodName { get; }

        public IReadOnlyList<StackFrameParameter> Parameters { get; }

        public string LocationPrefix { get; }

        public string DirectoryPath { get; }

        public string FileName { get; }

        public string LineNumber { get; }

        public StackFrame(
            string prefix,
            string parentType,
            string methodName,
            IReadOnlyList<StackFrameParameter> parameters,
            string locationPrefix,
            string directoryPath,
            string fileName,
            string lineNumber)
        {
            Prefix = prefix;
            ParentType = parentType;
            MethodName = methodName;
            Parameters = parameters;
            LocationPrefix = locationPrefix;
            DirectoryPath = directoryPath;
            FileName = fileName;
            LineNumber = lineNumber;
        }
    }

    internal partial class StackFrame
    {
        private static readonly Regex MethodMatcher =
            new Regex(@"(?<prefix>\S+) (?<name>.*?)(?<methodName>[^\.]+)\(");

        private static readonly Regex ParameterMatcher =
            new Regex(@"(?<type>.+? )(?<name>.+?)(?:(?<separator>, )|\))");

        private static readonly Regex FileMatcher =
            new Regex(@"(?<prefix>\S+?) (?<path>.*?)(?<file>[^\\/]+?(?:\.\w*)?):[^:]+? (?<line>\d+).*");

        public static StackFrame Parse(string stackFrame)
        {
            var methodMatch = MethodMatcher.Match(stackFrame);

            var parameterMatches = ParameterMatcher.Matches(stackFrame, methodMatch.Index + methodMatch.Length)
                .Cast<Match>()
                .ToArray();

            var fileMatch = FileMatcher.Match(
                stackFrame,
                parameterMatches.Length switch
                {
                    0 => methodMatch.Index + methodMatch.Length + 1,
                    _ => parameterMatches[parameterMatches.Length - 1].Index +
                         parameterMatches[parameterMatches.Length - 1].Length
                }
            );

            // Ensure everything was parsed successfully
            var isSuccessful =
                methodMatch.Success &&
                parameterMatches.All(m => m.Success) &&
                fileMatch.Success &&
                fileMatch.Index + fileMatch.Length == stackFrame.Length;

            if (!isSuccessful)
            {
                throw new FormatException("Failed to parse stack frame.");
            }

            var parameters = parameterMatches
                .Select(match => new StackFrameParameter(
                    match.Groups["type"].Value,
                    match.Groups["name"].Value,
                    match.Groups["separator"].Value.NullIfWhiteSpace()
                )).ToArray();

            return new StackFrame(
                methodMatch.Groups["prefix"].Value,
                methodMatch.Groups["name"].Value,
                methodMatch.Groups["methodName"].Value,
                parameters,
                fileMatch.Groups["prefix"].Value,
                fileMatch.Groups["path"].Value,
                fileMatch.Groups["file"].Value,
                fileMatch.Groups["line"].Value
            );
        }

        public static IReadOnlyList<StackFrame> ParseMany(string stackTrace) =>
            stackTrace.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(Parse).ToArray();
    }
}