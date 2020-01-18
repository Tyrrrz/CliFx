using System.Collections.Generic;
using CliFx.Domain;
using CliFx.Logic;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Logic
{
    [TestFixture]
    public class InputParsingLogicTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_ParseCommandInput()
        {
            yield return new TestCaseData(
                new string[0],
                new Dictionary<string, string>(),
                CommandInput.Empty
            );

            yield return new TestCaseData(
                new[] {"--option", "value"},
                new Dictionary<string, string>(),
                new CommandInput(new[]
                {
                    new CommandOptionInput("option", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"--option1", "value1", "--option2", "value2"},
                new Dictionary<string, string>(),
                new CommandInput(new[]
                {
                    new CommandOptionInput("option1", "value1"),
                    new CommandOptionInput("option2", "value2")
                })
            );

            yield return new TestCaseData(
                new[] {"--option", "value1", "value2"},
                new Dictionary<string, string>(),
                new CommandInput(new[]
                {
                    new CommandOptionInput("option", new[] {"value1", "value2"})
                })
            );

            yield return new TestCaseData(
                new[] {"--option", "value1", "--option", "value2"},
                new Dictionary<string, string>(),
                new CommandInput(new[]
                {
                    new CommandOptionInput("option", new[] {"value1", "value2"})
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value"},
                new Dictionary<string, string>(),
                new CommandInput(new[]
                {
                    new CommandOptionInput("a", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value1", "-b", "value2"},
                new Dictionary<string, string>(),
                new CommandInput(new[]
                {
                    new CommandOptionInput("a", "value1"),
                    new CommandOptionInput("b", "value2")
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value1", "value2"},
                new Dictionary<string, string>(),
                new CommandInput(new[]
                {
                    new CommandOptionInput("a", new[] {"value1", "value2"})
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value1", "-a", "value2"},
                new Dictionary<string, string>(),
                new CommandInput(new[]
                {
                    new CommandOptionInput("a", new[] {"value1", "value2"})
                })
            );

            yield return new TestCaseData(
                new[] {"--option1", "value1", "-b", "value2"},
                new Dictionary<string, string>(),
                new CommandInput(new[]
                {
                    new CommandOptionInput("option1", "value1"),
                    new CommandOptionInput("b", "value2")
                })
            );

            yield return new TestCaseData(
                new[] {"--switch"},
                new Dictionary<string, string>(),
                new CommandInput(new[]
                {
                    new CommandOptionInput("switch")
                })
            );

            yield return new TestCaseData(
                new[] {"--switch1", "--switch2"},
                new Dictionary<string, string>(),
                new CommandInput(new[]
                {
                    new CommandOptionInput("switch1"),
                    new CommandOptionInput("switch2")
                })
            );

            yield return new TestCaseData(
                new[] {"-s"},
                new Dictionary<string, string>(),
                new CommandInput(new[]
                {
                    new CommandOptionInput("s")
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "-b"},
                new Dictionary<string, string>(),
                new CommandInput(new[]
                {
                    new CommandOptionInput("a"),
                    new CommandOptionInput("b")
                })
            );

            yield return new TestCaseData(
                new[] {"-ab"},
                new Dictionary<string, string>(),
                new CommandInput(new[]
                {
                    new CommandOptionInput("a"),
                    new CommandOptionInput("b")
                })
            );

            yield return new TestCaseData(
                new[] {"-ab", "value"},
                new Dictionary<string, string>(),
                new CommandInput(new[]
                {
                    new CommandOptionInput("a"),
                    new CommandOptionInput("b", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"command"},
                new Dictionary<string, string>(),
                new CommandInput(new[] {"command"})
            );

            yield return new TestCaseData(
                new[] {"command", "--option", "value"},
                new Dictionary<string, string>(),
                new CommandInput(new[] {"command"}, new[]
                {
                    new CommandOptionInput("option", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"long", "command", "name"},
                new Dictionary<string, string>(),
                new CommandInput(new[] {"long", "command", "name"})
            );

            yield return new TestCaseData(
                new[] {"long", "command", "name", "--option", "value"},
                new Dictionary<string, string>(),
                new CommandInput(new[] {"long", "command", "name"}, new[]
                {
                    new CommandOptionInput("option", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"[debug]"},
                new Dictionary<string, string>(),
                new CommandInput(new string[0],
                    new[] {"debug"},
                    new CommandOptionInput[0])
            );

            yield return new TestCaseData(
                new[] {"[debug]", "[preview]"},
                new Dictionary<string, string>(),
                new CommandInput(new string[0],
                    new[] {"debug", "preview"},
                    new CommandOptionInput[0])
            );

            yield return new TestCaseData(
                new[] {"[debug]", "[preview]", "-o", "value"},
                new Dictionary<string, string>(),
                new CommandInput(new string[0],
                    new[] {"debug", "preview"},
                    new[]
                    {
                        new CommandOptionInput("o", "value")
                    })
            );

            yield return new TestCaseData(
                new[] {"command", "[debug]", "[preview]", "-o", "value"},
                new Dictionary<string, string>(),
                new CommandInput(new[] {"command"},
                    new[] {"debug", "preview"},
                    new[]
                    {
                        new CommandOptionInput("o", "value")
                    })
            );

            yield return new TestCaseData(
                new[] {"command", "[debug]", "[preview]", "-o", "value"},
                new Dictionary<string, string> {["envvar"] = "envvalue"},
                new CommandInput(new[] {"command"},
                    new[] {"debug", "preview"},
                    new[]
                    {
                        new CommandOptionInput("o", "value")
                    },
                    new Dictionary<string, string> {["envvar"] = "envvalue"})
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_ParseCommandInput))]
        public void ParseCommandInput_Test(IReadOnlyList<string> commandLineArguments, IReadOnlyDictionary<string, string> environmentVariables,
            CommandInput expectedCommandInput)
        {
            // Act
            var commandInput = InputParsingLogic.ParseCommandInput(commandLineArguments, environmentVariables);

            // Assert
            commandInput.Should().BeEquivalentTo(expectedCommandInput);
        }
    }
}