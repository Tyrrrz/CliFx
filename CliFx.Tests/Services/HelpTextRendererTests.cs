using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CliFx.Models;
using CliFx.Services;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Services
{
    [TestFixture]
    public partial class HelpTextRendererTests
    {
        private static HelpTextSource CreateHelpTextSource(IReadOnlyList<Type> availableCommandTypes, Type targetCommandType)
        {
            var commandSchemaResolver = new CommandSchemaResolver();

            var applicationMetadata = new ApplicationMetadata("TestApp", "testapp", "1.0", null);
            var availableCommandSchemas = commandSchemaResolver.GetCommandSchemas(availableCommandTypes);
            var targetCommandSchema = availableCommandSchemas.Single(s => s.Type == targetCommandType);

            return new HelpTextSource(applicationMetadata, availableCommandSchemas, targetCommandSchema);
        }

        private static IEnumerable<TestCaseData> GetTestCases_RenderHelpText()
        {
            yield return new TestCaseData(
                CreateHelpTextSource(
                    new[] {typeof(DefaultCommand), typeof(NamedCommand), typeof(NamedSubCommand)},
                    typeof(DefaultCommand)),

                new[]
                {
                    "Usage",
                    "[command]", "[options]",
                    "Options",
                    "-a|--option-a", "OptionA description.",
                    "-b|--option-b", "OptionB description.",
                    "-h|--help", "Shows help text.",
                    "--version", "Shows version information.",
                    "Commands",
                    "cmd", "NamedCommand description.",
                    "You can run", "to show help on a specific command."
                }
            );

            yield return new TestCaseData(
                CreateHelpTextSource(
                    new[] {typeof(DefaultCommand), typeof(NamedCommand), typeof(NamedSubCommand)},
                    typeof(NamedCommand)),

                new[]
                {
                    "Description",
                    "NamedCommand description.",
                    "Usage",
                    "cmd", "[command]", "[options]",
                    "Options",
                    "-c|--option-c", "OptionC description.",
                    "-d|--option-d", "OptionD description.",
                    "-h|--help", "Shows help text.",
                    "Commands",
                    "sub", "NamedSubCommand description.",
                    "You can run", "to show help on a specific command."
                }
            );

            yield return new TestCaseData(
                CreateHelpTextSource(
                    new[] {typeof(DefaultCommand), typeof(NamedCommand), typeof(NamedSubCommand)},
                    typeof(NamedSubCommand)),

                new[]
                {
                    "Description",
                    "NamedSubCommand description.",
                    "Usage",
                    "cmd sub", "[options]",
                    "Options",
                    "-e|--option-e", "OptionE description.",
                    "-h|--help", "Shows help text."
                }
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_RenderHelpText))]
        public void RenderHelpText_Test(HelpTextSource source, IReadOnlyList<string> expectedSubstrings)
        {
            // Arrange
            using (var stdout = new StringWriter())
            {
                var renderer = new HelpTextRenderer();
                var console = new VirtualConsole(stdout);

                // Act
                renderer.RenderHelpText(console, source);

                // Assert
                stdout.ToString().Should().ContainAll(expectedSubstrings);
            }
        }
    }
}