﻿using System.Collections.Generic;
using CliFx.Domain;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Domain
{
    [TestFixture]
    internal class CommandLineInputTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_Parse()
        {
            yield return new TestCaseData(
                new string[0],
                CommandLineInput.Empty
            );

            yield return new TestCaseData(
                new[] {"--option", "value"},
                new CommandLineInput(new[]
                {
                    new CommandOptionInput("option", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"--option1", "value1", "--option2", "value2"},
                new CommandLineInput(new[]
                {
                    new CommandOptionInput("option1", "value1"),
                    new CommandOptionInput("option2", "value2")
                })
            );

            yield return new TestCaseData(
                new[] {"--option", "value1", "value2"},
                new CommandLineInput(new[]
                {
                    new CommandOptionInput("option", new[] {"value1", "value2"})
                })
            );

            yield return new TestCaseData(
                new[] {"--option", "value1", "--option", "value2"},
                new CommandLineInput(new[]
                {
                    new CommandOptionInput("option", new[] {"value1", "value2"})
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value"},
                new CommandLineInput(new[]
                {
                    new CommandOptionInput("a", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value1", "-b", "value2"},
                new CommandLineInput(new[]
                {
                    new CommandOptionInput("a", "value1"),
                    new CommandOptionInput("b", "value2")
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value1", "value2"},
                new CommandLineInput(new[]
                {
                    new CommandOptionInput("a", new[] {"value1", "value2"})
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value1", "-a", "value2"},
                new CommandLineInput(new[]
                {
                    new CommandOptionInput("a", new[] {"value1", "value2"})
                })
            );

            yield return new TestCaseData(
                new[] {"--option1", "value1", "-b", "value2"},
                new CommandLineInput(new[]
                {
                    new CommandOptionInput("option1", "value1"),
                    new CommandOptionInput("b", "value2")
                })
            );

            yield return new TestCaseData(
                new[] {"--switch"},
                new CommandLineInput(new[]
                {
                    new CommandOptionInput("switch")
                })
            );

            yield return new TestCaseData(
                new[] {"--switch1", "--switch2"},
                new CommandLineInput(new[]
                {
                    new CommandOptionInput("switch1"),
                    new CommandOptionInput("switch2")
                })
            );

            yield return new TestCaseData(
                new[] {"-s"},
                new CommandLineInput(new[]
                {
                    new CommandOptionInput("s")
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "-b"},
                new CommandLineInput(new[]
                {
                    new CommandOptionInput("a"),
                    new CommandOptionInput("b")
                })
            );

            yield return new TestCaseData(
                new[] {"-ab"},
                new CommandLineInput(new[]
                {
                    new CommandOptionInput("a"),
                    new CommandOptionInput("b")
                })
            );

            yield return new TestCaseData(
                new[] {"-ab", "value"},
                new CommandLineInput(new[]
                {
                    new CommandOptionInput("a"),
                    new CommandOptionInput("b", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"command"},
                new CommandLineInput(new[] {"command"})
            );

            yield return new TestCaseData(
                new[] {"command", "--option", "value"},
                new CommandLineInput(new[] {"command"}, new[]
                {
                    new CommandOptionInput("option", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"cmd", "arg2", "arg3"},
                new CommandLineInput(new[] {"cmd", "arg2", "arg3"})
            );

            yield return new TestCaseData(
                new[] {"cmd", "arg2", "arg3", "--option", "value"},
                new CommandLineInput(new[] {"cmd", "arg2", "arg3"}, new[]
                {
                    new CommandOptionInput("option", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"[debug]"},
                new CommandLineInput(
                    new[] {"debug"},
                    new string[0],
                    new CommandOptionInput[0])
            );

            yield return new TestCaseData(
                new[] {"[debug]", "[preview]"},
                new CommandLineInput(
                    new[] {"debug", "preview"},
                    new string[0],
                    new CommandOptionInput[0])
            );

            yield return new TestCaseData(
                new[] {"[debug]", "[preview]", "-o", "value"},
                new CommandLineInput(
                    new[] {"debug", "preview"},
                    new string[0],
                    new[]
                    {
                        new CommandOptionInput("o", "value")
                    })
            );

            yield return new TestCaseData(
                new[] {"command", "[debug]", "[preview]", "-o", "value"},
                new CommandLineInput(
                    new[] {"debug", "preview"},
                    new[] {"command"},
                    new[]
                    {
                        new CommandOptionInput("o", "value")
                    })
            );

            yield return new TestCaseData(
                new[] {"command", "[debug]", "[preview]", "-o", "value"},
                new CommandLineInput(
                    new[] {"debug", "preview"},
                    new[] {"command"},
                    new[]
                    {
                        new CommandOptionInput("o", "value")
                    })
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_Parse))]
        public void Parse_Test(IReadOnlyList<string> commandLineArguments, CommandLineInput expectedResult)
        {
            // Act
            var result = CommandLineInput.Parse(commandLineArguments);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }
    }
}