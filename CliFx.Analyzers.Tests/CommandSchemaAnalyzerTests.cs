using System.Collections.Generic;
using CliFx.Analyzers.Tests.Internal;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests
{
    public class CommandSchemaAnalyzerTests
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new CommandSchemaAnalyzer();

        public static IEnumerable<object[]> GetValidCases()
        {
            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Non-command type",
                    Analyzer.SupportedDiagnostics,

                    // language=cs
                    @"
public class Foo
{
    public int Bar { get; set; } = 5;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Command implements interface and has attribute",
                    Analyzer.SupportedDiagnostics,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Command doesn't have an attribute but is an abstract type",
                    Analyzer.SupportedDiagnostics,

                    // language=cs
                    @"
public abstract class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Parameters with unique order",
                    Analyzer.SupportedDiagnostics,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(13)]
    public string ParamA { get; set; }
    
    [CommandParameter(15)]
    public string ParamB { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Parameters with unique names",
                    Analyzer.SupportedDiagnostics,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(13, Name = ""foo"")]
    public string ParamA { get; set; }
    
    [CommandParameter(15, Name = ""bar"")]
    public string ParamB { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Single non-scalar parameter",
                    Analyzer.SupportedDiagnostics,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(1)]
    public string ParamA { get; set; }
    
    [CommandParameter(2)]
    public HashSet<string> ParamB { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Non-scalar parameter is last in order",
                    Analyzer.SupportedDiagnostics,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(1)]
    public string ParamA { get; set; }
    
    [CommandParameter(2)]
    public IReadOnlyList<string> ParamB { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Option with a proper name",
                    Analyzer.SupportedDiagnostics,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandOption(""foo"")]
    public string Param { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Option with a proper name and short name",
                    Analyzer.SupportedDiagnostics,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandOption(""foo"", 'f')]
    public string Param { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Options with unique names",
                    Analyzer.SupportedDiagnostics,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandOption(""foo"")]
    public string ParamA { get; set; }
    
    [CommandOption(""bar"")]
    public string ParamB { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Options with unique short names",
                    Analyzer.SupportedDiagnostics,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandOption('f')]
    public string ParamA { get; set; }
    
    [CommandOption('x')]
    public string ParamB { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Options with unique environment variable names",
                    Analyzer.SupportedDiagnostics,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandOption('a', EnvironmentVariableName = ""env_var_a"")]
    public string ParamA { get; set; }
    
    [CommandOption('b', EnvironmentVariableName = ""env_var_b"")]
    public string ParamB { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };
        }

        public static IEnumerable<object[]> GetInvalidCases()
        {
            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Command is missing the attribute",
                    DiagnosticDescriptors.CliFx0002,

                    // language=cs
                    @"
public class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Command doesn't implement the interface",
                    DiagnosticDescriptors.CliFx0001,

                    // language=cs
                    @"
[Command]
public class MyCommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Parameters with duplicate order",
                    DiagnosticDescriptors.CliFx0021,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(13)]
    public string ParamA { get; set; }
    
    [CommandParameter(13)]
    public string ParamB { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Parameters with duplicate names",
                    DiagnosticDescriptors.CliFx0022,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(13, Name = ""foo"")]
    public string ParamA { get; set; }
    
    [CommandParameter(15, Name = ""foo"")]
    public string ParamB { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Multiple non-scalar parameters",
                    DiagnosticDescriptors.CliFx0023,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(1)]
    public IReadOnlyList<string> ParamA { get; set; }
    
    [CommandParameter(2)]
    public HashSet<string> ParamB { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Non-last non-scalar parameter",
                    DiagnosticDescriptors.CliFx0024,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(1)]
    public IReadOnlyList<string> ParamA { get; set; }
    
    [CommandParameter(2)]
    public string ParamB { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Option with an empty name",
                    DiagnosticDescriptors.CliFx0041,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandOption("""")]
    public string Param { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Option with a name which is too short",
                    DiagnosticDescriptors.CliFx0042,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandOption(""a"")]
    public string Param { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Options with duplicate names",
                    DiagnosticDescriptors.CliFx0043,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandOption(""foo"")]
    public string ParamA { get; set; }
    
    [CommandOption(""foo"")]
    public string ParamB { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Options with duplicate short names",
                    DiagnosticDescriptors.CliFx0044,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandOption('f')]
    public string ParamA { get; set; }
    
    [CommandOption('f')]
    public string ParamB { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Options with duplicate environment variable names",
                    DiagnosticDescriptors.CliFx0045,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandOption('a', EnvironmentVariableName = ""env_var"")]
    public string ParamA { get; set; }
    
    [CommandOption('b', EnvironmentVariableName = ""env_var"")]
    public string ParamB { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };
        }

        [Theory]
        [MemberData(nameof(GetValidCases))]
        public void Valid(AnalyzerTestCase testCase)
        {
            Analyzer.Should().NotProduceDiagnostics(testCase);
        }

        [Theory]
        [MemberData(nameof(GetInvalidCases))]
        public void Invalid(AnalyzerTestCase testCase)
        {
            Analyzer.Should().ProduceDiagnostics(testCase);
        }
    }
}