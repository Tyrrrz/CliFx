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
                    DiagnosticDescriptors.CliFx0003,

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
                    "Non-command type",
                    DiagnosticDescriptors.CliFx0002,

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
                    DiagnosticDescriptors.CliFx0003,

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
                    "Command implements interface and has attribute",
                    DiagnosticDescriptors.CliFx0002,

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
                    "Command is an abstract type",
                    DiagnosticDescriptors.CliFx0003,

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
                    DiagnosticDescriptors.CliFx0004,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(13)]
    public string? ParamA { get; set; }
    
    [CommandParameter(15)]
    public string? ParamB { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Parameters with unique names",
                    DiagnosticDescriptors.CliFx0005,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(13, Name = ""foo"")]
    public string? ParamA { get; set; }
    
    [CommandParameter(15, Name = ""bar"")]
    public string? ParamB { get; set; }

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
                    DiagnosticDescriptors.CliFx0003,

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
                    DiagnosticDescriptors.CliFx0002,

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
                    DiagnosticDescriptors.CliFx0004,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(13)]
    public string? ParamA { get; set; }
    
    [CommandParameter(13)]
    public string? ParamB { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Parameters with duplicate names",
                    DiagnosticDescriptors.CliFx0005,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    [CommandParameter(13, Name = ""foo"")]
    public string? ParamA { get; set; }
    
    [CommandParameter(15, Name = ""foo"")]
    public string? ParamB { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
                )
            };
        }

        [Theory]
        [MemberData(nameof(GetValidCases))]
        public void Valid(AnalyzerTestCase testCase) =>
            Analyzer.Should().NotProduceDiagnostics(testCase);

        [Theory]
        [MemberData(nameof(GetInvalidCases))]
        public void Invalid(AnalyzerTestCase testCase) =>
            Analyzer.Should().ProduceDiagnostics(testCase);
    }
}