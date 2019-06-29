using System.Collections.Generic;
using CliFx.Models;
using CliFx.Services;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public class CommandOptionParserTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_ParseOptions()
        {
            yield return new TestCaseData(new string[0], CommandOptionSet.Empty);

            yield return new TestCaseData(
                new[] {"--option", "value"},
                new CommandOptionSet(new[]
                {
                    new CommandOption("option", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"--option1", "value1", "--option2", "value2"},
                new CommandOptionSet(new[]
                {
                    new CommandOption("option1", "value1"),
                    new CommandOption("option2", "value2")
                })
            );

            yield return new TestCaseData(
                new[] {"--option", "value1", "value2"},
                new CommandOptionSet(new[]
                {
                    new CommandOption("option", new[] {"value1", "value2"})
                })
            );

            yield return new TestCaseData(
                new[] {"--option", "value1", "--option", "value2"},
                new CommandOptionSet(new[]
                {
                    new CommandOption("option", new[] {"value1", "value2"})
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value"},
                new CommandOptionSet(new[]
                {
                    new CommandOption("a", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value1", "-b", "value2"},
                new CommandOptionSet(new[]
                {
                    new CommandOption("a", "value1"),
                    new CommandOption("b", "value2")
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value1", "value2"},
                new CommandOptionSet(new[]
                {
                    new CommandOption("a", new[] {"value1", "value2"})
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value1", "-a", "value2"},
                new CommandOptionSet(new[]
                {
                    new CommandOption("a", new[] {"value1", "value2"})
                })
            );

            yield return new TestCaseData(
                new[] {"--option1", "value1", "-b", "value2"},
                new CommandOptionSet(new[]
                {
                    new CommandOption("option1", "value1"),
                    new CommandOption("b", "value2")
                })
            );

            yield return new TestCaseData(
                new[] {"--switch"},
                new CommandOptionSet(new[]
                {
                    new CommandOption("switch")
                })
            );

            yield return new TestCaseData(
                new[] {"--switch1", "--switch2"},
                new CommandOptionSet(new[]
                {
                    new CommandOption("switch1"),
                    new CommandOption("switch2")
                })
            );

            yield return new TestCaseData(
                new[] {"-s"},
                new CommandOptionSet(new[]
                {
                    new CommandOption("s")
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "-b"},
                new CommandOptionSet(new[]
                {
                    new CommandOption("a"),
                    new CommandOption("b")
                })
            );

            yield return new TestCaseData(
                new[] {"-ab"},
                new CommandOptionSet(new[]
                {
                    new CommandOption("a"),
                    new CommandOption("b")
                })
            );

            yield return new TestCaseData(
                new[] {"-ab", "value"},
                new CommandOptionSet(new[]
                {
                    new CommandOption("a"),
                    new CommandOption("b", "value")
                })
            );

            yield return new TestCaseData(
                new[] {"command"},
                new CommandOptionSet("command")
            );

            yield return new TestCaseData(
                new[] {"command", "--option", "value"},
                new CommandOptionSet("command", new[]
                {
                    new CommandOption("option", "value")
                })
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_ParseOptions))]
        public void ParseOptions_Test(IReadOnlyList<string> commandLineArguments, CommandOptionSet expectedCommandOptionSet)
        {
            // Arrange
            var parser = new CommandOptionParser();

            // Act
            var optionSet = parser.ParseOptions(commandLineArguments);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(optionSet.CommandName, Is.EqualTo(expectedCommandOptionSet.CommandName), "Command name");
                Assert.That(optionSet.Options.Count, Is.EqualTo(expectedCommandOptionSet.Options.Count), "Option count");

                for (var i = 0; i < optionSet.Options.Count; i++)
                {
                    Assert.That(optionSet.Options[i].Name, Is.EqualTo(expectedCommandOptionSet.Options[i].Name),
                        $"Option[{i}] name");

                    Assert.That(optionSet.Options[i].Values, Is.EqualTo(expectedCommandOptionSet.Options[i].Values),
                        $"Option[{i}] values");
                }
            });
        }
    }
}