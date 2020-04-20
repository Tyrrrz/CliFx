using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace CliFx.Analyzers.Tests
{
    public class CommandSchemaAnalyzerTests : AnalyzerTestsBase
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new CommandSchemaAnalyzer();

        public static  IEnumerable<object[]> GetValidCodes()
        {
            yield return new object[]
            {
                Descriptor.CliFx0002,

                // language=cs
                @"
[Command]
public class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}"
            };

            yield return new object[]
            {
                Descriptor.CliFx0003,

                // language=cs
                @"
[Command]
public class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}"
            };

            yield return new object[]
            {
                Descriptor.CliFx0002,

                // language=cs
                @"
public abstract class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}"
            };
        }

        public static IEnumerable<object[]> GetInvalidCodes()
        {
            yield return new object[]
            {
                Descriptor.CliFx0002,

                // language=cs
                @"
public class MyCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}"
            };

            yield return new object[]
            {
                Descriptor.CliFx0003,

                // language=cs
                @"
[Command]
public class MyCommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
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