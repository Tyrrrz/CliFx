using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CliFx.Models;
using CliFx.Services;
using CliFx.Tests.TestCommands;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Services
{
    [TestFixture]
    public class HelpTextRendererTests
    {
        private static HelpTextSource CreateHelpTextSource(IReadOnlyList<Type> availableCommandTypes, Type targetCommandType)
        {
            var commandSchemaResolver = new CommandSchemaResolver(new CommandArgumentSchemasValidator());

            var applicationMetadata = new ApplicationMetadata("TestApp", "testapp", "1.0", null);
            var availableCommandSchemas = commandSchemaResolver.GetCommandSchemas(availableCommandTypes);
            var targetCommandSchema = availableCommandSchemas.Single(s => s.Type == targetCommandType);

            return new HelpTextSource(applicationMetadata, availableCommandSchemas, targetCommandSchema);
        }

        private static IEnumerable<TestCaseData> GetTestCases_RenderHelpText()
        {
            yield return new TestCaseData(
                CreateHelpTextSource(
                    new[] {typeof(HelpDefaultCommand), typeof(HelpNamedCommand), typeof(HelpSubCommand)},
                    typeof(HelpDefaultCommand)),

                new[]
                {
                    "Description",
                    "HelpDefaultCommand description.",
                    "Usage",
                    "[command]", "[options]",
                    "Options",
                    "-a|--option-a", "OptionA description.",
                    "-b|--option-b", "OptionB description.",
                    "-h|--help", "Shows help text.",
                    "--version", "Shows version information.",
                    "Commands",
                    "cmd", "HelpNamedCommand description.",
                    "You can run", "to show help on a specific command."
                },

                new string[0]
            );

            yield return new TestCaseData(
                CreateHelpTextSource(
                    new[] {typeof(HelpDefaultCommand), typeof(HelpNamedCommand), typeof(HelpSubCommand)},
                    typeof(HelpNamedCommand)),

                new[]
                {
                    "Description",
                    "HelpNamedCommand description.",
                    "Usage",
                    "cmd", "[command]", "[options]",
                    "Options",
                    "-c|--option-c", "OptionC description.",
                    "-d|--option-d", "OptionD description.",
                    "-h|--help", "Shows help text.",
                    "Commands",
                    "sub", "HelpSubCommand description.",
                    "You can run", "to show help on a specific command."
                },

                new string[0]
            );

            yield return new TestCaseData(
                CreateHelpTextSource(
                    new[] {typeof(HelpDefaultCommand), typeof(HelpNamedCommand), typeof(HelpSubCommand)},
                    typeof(HelpSubCommand)),

                new[]
                {
                    "Description",
                    "HelpSubCommand description.",
                    "Usage",
                    "cmd sub", "[options]",
                    "Options",
                    "-e|--option-e", "OptionE description.",
                    "-h|--help", "Shows help text."
                },

                new string[0]
            );
            
            yield return new TestCaseData(
                CreateHelpTextSource(
                    new[] {typeof(ArgumentCommand)},
                    typeof(ArgumentCommand)),

                new[]
                {
                    "Description",
                    "Command using positional arguments",
                    "Usage",
                    "arg cmd", "<first>", "[<secondargument>]", "[<third list>]", "[options]",
                    "Arguments",
                    "* first",
                    "secondargument",
                    "third list", "A list of numbers",
                    "Options",
                    "-o|--option",
                    "-h|--help", "Shows help text."
                },

                new string[0]
            );

            yield return new TestCaseData(
                CreateHelpTextSource(
                    new[] { typeof(AllRequiredOptionsCommand) },
                    typeof(AllRequiredOptionsCommand)),

                new[]
                {
                    "Description",
                    "AllRequiredOptionsCommand description.",
                    "Usage",
                    "testapp allrequired -f <value> -g <value>"
                },

                new []
                {
                    "[options]"
                }
            );

            yield return new TestCaseData(
                CreateHelpTextSource(
                    new[] { typeof(SomeRequiredOptionsCommand) },
                    typeof(SomeRequiredOptionsCommand)),

                new[]
                {
                    "Description",
                    "SomeRequiredOptionsCommand description.",
                    "Usage",
                    "testapp somerequired -f <value> [options]"
                },

                new string[0]
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_RenderHelpText))]
        public void RenderHelpText_Test(HelpTextSource source,
            IReadOnlyList<string> expectedSubstrings,
            IReadOnlyList<string> notExpectedSubstrings)
        {
            // Arrange
            using var stdout = new StringWriter();

            var console = new VirtualConsole(stdout);
            var renderer = new HelpTextRenderer();

            // Act
            renderer.RenderHelpText(console, source);

            // Assert
            stdout.ToString().Should().ContainAll(expectedSubstrings);
            if (notExpectedSubstrings != null && notExpectedSubstrings.Any())
            {
                stdout.ToString().Should().NotContainAll(notExpectedSubstrings);
            }
        }
    }
}