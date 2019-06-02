using System.Collections.Generic;
using CliFx.Models;
using CliFx.Services;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public class CommandOptionParserTests
    {
        private static IEnumerable<TestCaseData> GetData_ParseOptions()
        {
            yield return new TestCaseData(new string[0], CommandOptionSet.Empty);

            yield return new TestCaseData(
                new[] {"--option", "value"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"option", "value"}
                })
            );

            yield return new TestCaseData(
                new[] {"--option1", "value1", "--option2", "value2"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"option1", "value1"},
                    {"option2", "value2"}
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"a", "value"}
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "value1", "-b", "value2"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"a", "value1"},
                    {"b", "value2"}
                })
            );

            yield return new TestCaseData(
                new[] {"--option1", "value1", "-b", "value2"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"option1", "value1"},
                    {"b", "value2"}
                })
            );

            yield return new TestCaseData(
                new[] {"--switch"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"switch", null}
                })
            );

            yield return new TestCaseData(
                new[] {"--switch1", "--switch2"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"switch1", null},
                    {"switch2", null}
                })
            );

            yield return new TestCaseData(
                new[] {"-s"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"s", null}
                })
            );

            yield return new TestCaseData(
                new[] {"-a", "-b"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"a", null},
                    {"b", null}
                })
            );

            yield return new TestCaseData(
                new[] {"-ab"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"a", null},
                    {"b", null}
                })
            );

            yield return new TestCaseData(
                new[] {"command"},
                new CommandOptionSet("command")
            );

            yield return new TestCaseData(
                new[] {"command", "--option", "value"},
                new CommandOptionSet("command", new Dictionary<string, string>
                {
                    {"option", "value"}
                })
            );
        }

        [Test]
        [TestCaseSource(nameof(GetData_ParseOptions))]
        public void ParseOptions_Test(IReadOnlyList<string> commandLineArguments, CommandOptionSet expectedCommandOptionSet)
        {
            // Arrange
            var parser = new CommandOptionParser();

            // Act
            var optionSet = parser.ParseOptions(commandLineArguments);

            // Assert
            Assert.That(optionSet.CommandName, Is.EqualTo(expectedCommandOptionSet.CommandName), "Command name");
            Assert.That(optionSet.Options, Is.EqualTo(expectedCommandOptionSet.Options), "Options");
        }
    }
}