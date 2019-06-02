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
            yield return new TestCaseData(
                new string[0],
                CommandOptionSet.Empty
            ).SetName("No arguments");

            yield return new TestCaseData(
                new[] {"--argument", "value"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"argument", "value"}
                })
            ).SetName("Single argument");

            yield return new TestCaseData(
                new[] {"--argument1", "value1", "--argument2", "value2", "--argument3", "value3"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"argument1", "value1"},
                    {"argument2", "value2"},
                    {"argument3", "value3"}
                })
            ).SetName("Multiple arguments");

            yield return new TestCaseData(
                new[] {"-a", "value"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"a", "value"}
                })
            ).SetName("Single short argument");

            yield return new TestCaseData(
                new[] {"-a", "value1", "-b", "value2", "-c", "value3"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"a", "value1"},
                    {"b", "value2"},
                    {"c", "value3"}
                })
            ).SetName("Multiple short arguments");

            yield return new TestCaseData(
                new[] {"--argument1", "value1", "-b", "value2", "--argument3", "value3"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"argument1", "value1"},
                    {"b", "value2"},
                    {"argument3", "value3"}
                })
            ).SetName("Multiple mixed arguments");

            yield return new TestCaseData(
                new[] {"--switch"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"switch", null}
                })
            ).SetName("Single switch");

            yield return new TestCaseData(
                new[] {"--switch1", "--switch2", "--switch3"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"switch1", null},
                    {"switch2", null},
                    {"switch3", null}
                })
            ).SetName("Multiple switches");

            yield return new TestCaseData(
                new[] {"-s"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"s", null}
                })
            ).SetName("Single short switch");

            yield return new TestCaseData(
                new[] {"-a", "-b", "-c"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"a", null},
                    {"b", null},
                    {"c", null}
                })
            ).SetName("Multiple short switches");

            yield return new TestCaseData(
                new[] {"-abc"},
                new CommandOptionSet(new Dictionary<string, string>
                {
                    {"a", null},
                    {"b", null},
                    {"c", null}
                })
            ).SetName("Multiple stacked short switches");

            yield return new TestCaseData(
                new[] {"command"},
                new CommandOptionSet("command")
            ).SetName("No arguments (with command name)");

            yield return new TestCaseData(
                new[] {"command", "--argument", "value"},
                new CommandOptionSet("command", new Dictionary<string, string>
                {
                    {"argument", "value"}
                })
            ).SetName("Single argument (with command name)");
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
            Assert.That(optionSet.CommandName, Is.EqualTo(expectedCommandOptionSet.CommandName));
            Assert.That(optionSet.Options, Is.EqualTo(expectedCommandOptionSet.Options));
        }
    }
}