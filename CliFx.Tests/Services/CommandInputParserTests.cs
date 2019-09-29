using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using CliFx.Models;
using CliFx.Services;
using CliFx.Tests.Stubs;

namespace CliFx.Tests.Services
{
    [TestFixture]
    public class CommandInputParserTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_ParseCommandInput()
        {
            yield return new TestCaseData(new string[0], CommandInput.Empty, new EmptyEnvironmentVariablesProviderStub());

            yield return new TestCaseData(
                new[] { "--option", "value" },
                new CommandInput(new[]
                {
                    new CommandOptionInput("option", "value")
                }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "--option1", "value1", "--option2", "value2" },
                new CommandInput(new[]
                {
                    new CommandOptionInput("option1", "value1"),
                    new CommandOptionInput("option2", "value2")
                }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "--option", "value1", "value2" },
                new CommandInput(new[]
                {
                    new CommandOptionInput("option", new[] {"value1", "value2"})
                }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "--option", "value1", "--option", "value2" },
                new CommandInput(new[]
                {
                    new CommandOptionInput("option", new[] {"value1", "value2"})
                }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "-a", "value" },
                new CommandInput(new[]
                {
                    new CommandOptionInput("a", "value")
                }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "-a", "value1", "-b", "value2" },
                new CommandInput(new[]
                {
                    new CommandOptionInput("a", "value1"),
                    new CommandOptionInput("b", "value2")
                }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "-a", "value1", "value2" },
                new CommandInput(new[]
                {
                    new CommandOptionInput("a", new[] {"value1", "value2"})
                }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "-a", "value1", "-a", "value2" },
                new CommandInput(new[]
                {
                    new CommandOptionInput("a", new[] {"value1", "value2"})
                }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "--option1", "value1", "-b", "value2" },
                new CommandInput(new[]
                {
                    new CommandOptionInput("option1", "value1"),
                    new CommandOptionInput("b", "value2")
                }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "--switch" },
                new CommandInput(new[]
                {
                    new CommandOptionInput("switch")
                }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "--switch1", "--switch2" },
                new CommandInput(new[]
                {
                    new CommandOptionInput("switch1"),
                    new CommandOptionInput("switch2")
                }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "-s" },
                new CommandInput(new[]
                {
                    new CommandOptionInput("s")
                }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "-a", "-b" },
                new CommandInput(new[]
                {
                    new CommandOptionInput("a"),
                    new CommandOptionInput("b")
                }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "-ab" },
                new CommandInput(new[]
                {
                    new CommandOptionInput("a"),
                    new CommandOptionInput("b")
                }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "-ab", "value" },
                new CommandInput(new[]
                {
                    new CommandOptionInput("a"),
                    new CommandOptionInput("b", "value")
                }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "command" },
                new CommandInput("command"),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "command", "--option", "value" },
                new CommandInput("command", new[]
                {
                    new CommandOptionInput("option", "value")
                }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "long", "command", "name" },
                new CommandInput("long command name"),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "long", "command", "name", "--option", "value" },
                new CommandInput("long command name", new[]
                {
                    new CommandOptionInput("option", "value")
                }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "[debug]" },
                new CommandInput(null,
                    new[] { "debug" },
                    new CommandOptionInput[0]),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "[debug]", "[preview]" },
                new CommandInput(null,
                    new[] { "debug", "preview" },
                    new CommandOptionInput[0]),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "[debug]", "[preview]", "-o", "value" },
                new CommandInput(null,
                    new[] { "debug", "preview" },
                    new[]
                    {
                        new CommandOptionInput("o", "value")
                    }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "command", "[debug]", "[preview]", "-o", "value" },
                new CommandInput("command",
                    new[] { "debug", "preview" },
                    new[]
                    {
                        new CommandOptionInput("o", "value")
                    }),
                new EmptyEnvironmentVariablesProviderStub()
            );

            yield return new TestCaseData(
                new[] { "command", "[debug]", "[preview]", "-o", "value" },
                new CommandInput("command",
                    new[] { "debug", "preview" },
                    new[]
                    {
                        new CommandOptionInput("o", "value")
                    },
                    EnvironmentVariablesProviderStub.EnvironmentVariables),
                new EnvironmentVariablesProviderStub()
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_ParseCommandInput))]
        public void ParseCommandInput_Test(IReadOnlyList<string> commandLineArguments,
            CommandInput expectedCommandInput, IEnvironmentVariablesProvider environmentVariablesProvider)
        {
            // Arrange
            var parser = new CommandInputParser(environmentVariablesProvider);

            // Act
            var commandInput = parser.ParseCommandInput(commandLineArguments);

            // Assert
            commandInput.Should().BeEquivalentTo(expectedCommandInput);
        }
    }
}