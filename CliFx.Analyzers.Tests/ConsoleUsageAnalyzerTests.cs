using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests
{
    public class ConsoleUsageAnalyzerTests : AnalyzerTestsBase
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new ConsoleUsageAnalyzer();

        public static  IEnumerable<object[]> GetValidCodes()
        {
            yield return new object[]
            {
                Descriptor.CliFx0001,

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
            };

            yield return new object[]
            {
                Descriptor.CliFx0001,

                // language=cs
                @"
[Command]
public class MyCommand : ICommand
{
    public void SomeOtherMethod() => Console.WriteLine(""Test"");

    public ValueTask ExecuteAsync(IConsole console) => default;
}"
            };
        }

        public static IEnumerable<object[]> GetInvalidCodes()
        {
            yield return new object[]
            {
                Descriptor.CliFx0001,

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
            };
        }

        [Theory]
        [MemberData(nameof(GetValidCodes))]
        public void Positive(DiagnosticDescriptor diagnostic, string code) =>
            AssertCodeValid(Analyzer, diagnostic, code);

        [Theory]
        [MemberData(nameof(GetInvalidCodes))]
        public void Negative(DiagnosticDescriptor diagnostic, string code) =>
            AssertCodeInvalid(Analyzer, diagnostic, code);
    }
}