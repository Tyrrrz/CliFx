using System.Collections.Generic;
using CliFx.Analyzers.Tests.Internal;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests
{
    public class ConsoleUsageAnalyzerTests
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new ConsoleUsageAnalyzer();

        public static IEnumerable<object[]> GetValidCases()
        {
            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Using console abstraction",
                    Analyzer.SupportedDiagnostics,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""Hello world"");
        return default;
    }
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Console abstraction is not available in scope",
                    Analyzer.SupportedDiagnostics,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    public void SomeOtherMethod() => Console.WriteLine(""Test"");

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
                    "Not using available console abstraction in the ExecuteAsync method",
                    Analyzer.SupportedDiagnostics,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        Console.WriteLine(""Hello world"");
        return default;
    }
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Not using available console abstraction in the ExecuteAsync method when writing stderr",
                    Analyzer.SupportedDiagnostics,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        Console.Error.WriteLine(""Hello world"");
        return default;
    }
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Not using available console abstraction while referencing System.Console by full name",
                    Analyzer.SupportedDiagnostics,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        System.Console.Error.WriteLine(""Hello world"");
        return default;
    }
}"
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Not using available console abstraction in another method",
                    DiagnosticDescriptors.CliFx0100,

                    // language=cs
                    @"
[Command]
public class MyCommand : ICommand
{
    public void SomeOtherMethod(IConsole console) => Console.WriteLine(""Test"");

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