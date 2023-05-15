using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CliFx.Utils.Extensions;

namespace CliFx.Utils;

internal class StackFrameParameter
{
    public string Type { get; }

    public string? Name { get; }

    public StackFrameParameter(string type, string? name)
    {
        Type = type;
        Name = name;
    }
}

internal partial class StackFrame
{
    public string ParentType { get; }

    public string MethodName { get; }

    public IReadOnlyList<StackFrameParameter> Parameters { get; }

    public string? FilePath { get; }

    public string? LineNumber { get; }

    public StackFrame(
        string parentType,
        string methodName,
        IReadOnlyList<StackFrameParameter> parameters,
        string? filePath,
        string? lineNumber)
    {
        ParentType = parentType;
        MethodName = methodName;
        Parameters = parameters;
        FilePath = filePath;
        LineNumber = lineNumber;
    }
}

internal partial class StackFrame
{
    private const string Space = @"[\x20\t]";
    private const string NotSpace = @"[^\x20\t]";

    // Taken from https://github.com/atifaziz/StackTraceParser
    private static readonly Regex Pattern = new(
        $$"""
        ^
        {{Space}}*
        \w+ {{Space}}+
        (?<frame>
            (?<type> {{NotSpace}}+ ) \.
            (?<method> {{NotSpace}}+? ) {{Space}}*
            (?<params>  \( ( {{Space}}* \)
                           |                    (?<pt> .+?) {{Space}}+ (?<pn> .+?)
                             (, {{Space}}* (?<pt> .+?) {{Space}}+ (?<pn> .+?) )* \) ) )
            ( {{Space}}+
                ( # Microsoft .NET stack traces
                \w+ {{Space}}+
                (?<file> ( [a-z] \: # Windows rooted path starting with a drive letter
                         | / )      # Unix rooted path starting with a forward-slash
                         .+? )
                \: \w+ {{Space}}+
                (?<line> [0-9]+ ) \p{P}?
                | # Mono stack traces
                \[0x[0-9a-f]+\] {{Space}}+ \w+ {{Space}}+
                <(?<file> [^>]+ )>
                :(?<line> [0-9]+ )
                )
            )?
        )
        \s*
        $
        """,
        RegexOptions.IgnoreCase |
        RegexOptions.Multiline |
        RegexOptions.ExplicitCapture |
        RegexOptions.CultureInvariant |
        RegexOptions.IgnorePatternWhitespace,
        TimeSpan.FromSeconds(5)
    );

    public static IEnumerable<StackFrame> ParseTrace(string stackTrace)
    {
        var matches = Pattern.Matches(stackTrace).ToArray();

        if (matches.Length <= 0 || matches.Any(m => !m.Success))
        {
            // If parsing fails, we include the original stacktrace in the
            // exception so that it's shown to the user.
            throw new FormatException(
                $"""
                Could not parse stacktrace:
                {stackTrace}
                """
            );
        }

        return from m in matches
            select m.Groups
            into groups
            let pt = groups["pt"].Captures
            let pn = groups["pn"].Captures
            select new StackFrame(
                groups["type"].Value,
                groups["method"].Value,
                (
                    from i in Enumerable.Range(0, pt.Count)
                    select new StackFrameParameter(pt[i].Value, pn[i].Value.NullIfWhiteSpace())
                ).ToArray(),
                groups["file"].Value.NullIfWhiteSpace(),
                groups["line"].Value.NullIfWhiteSpace()
            );
    }
}