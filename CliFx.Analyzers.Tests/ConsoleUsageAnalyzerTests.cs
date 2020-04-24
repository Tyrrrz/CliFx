﻿using System.Collections.Generic;
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
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Method doesn't have console abstraction available",
                    Descriptor.CliFx0001,

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
                )
            };

            yield return new object[]
            {
                new AnalyzerTestCase(
                    "Not using available console abstraction in the ExecuteAsync method (nested)",
                    Descriptor.CliFx0001,

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
                    "Not using available console abstraction in another method",
                    Descriptor.CliFx0001,

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
            AssertAnalyzer.ValidCode(Analyzer, testCase);

        [Theory]
        [MemberData(nameof(GetInvalidCases))]
        public void Invalid(AnalyzerTestCase testCase) =>
            AssertAnalyzer.InvalidCode(Analyzer, testCase);
    }
}