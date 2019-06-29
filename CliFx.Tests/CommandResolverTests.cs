using System.Collections.Generic;
using CliFx.Exceptions;
using CliFx.Models;
using CliFx.Services;
using CliFx.Tests.TestObjects;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public class CommandResolverTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_ResolveCommand()
        {
            yield return new TestCaseData(
                new CommandOptionSet(new[]
                {
                    new CommandOption("int", "13")
                }),
                new TestCommand {IntOption = 13}
            );

            yield return new TestCaseData(
                new CommandOptionSet(new[]
                {
                    new CommandOption("int", "13"),
                    new CommandOption("str", "hello world")
                }),
                new TestCommand {IntOption = 13, StringOption = "hello world"}
            );

            yield return new TestCaseData(
                new CommandOptionSet(new[]
                {
                    new CommandOption("i", "13")
                }),
                new TestCommand {IntOption = 13}
            );

            yield return new TestCaseData(
                new CommandOptionSet("command", new[]
                {
                    new CommandOption("int", "13")
                }),
                new TestCommand {IntOption = 13}
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_ResolveCommand))]
        public void ResolveCommand_Test(CommandOptionSet commandOptionSet, TestCommand expectedCommand)
        {
            // Arrange
            var typeProvider = new TypeProvider(typeof(TestCommand));
            var optionConverter = new CommandOptionConverter();

            var resolver = new CommandResolver(typeProvider, optionConverter);

            // Act
            var command = resolver.ResolveCommand(commandOptionSet) as TestCommand;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(command, Is.Not.Null);
                Assert.That(command.StringOption, Is.EqualTo(expectedCommand.StringOption), nameof(command.StringOption));
                Assert.That(command.IntOption, Is.EqualTo(expectedCommand.IntOption), nameof(command.IntOption));
            });
        }

        private static IEnumerable<TestCaseData> GetTestCases_ResolveCommand_IsRequired()
        {
            yield return new TestCaseData(CommandOptionSet.Empty);

            yield return new TestCaseData(
                new CommandOptionSet(new[]
                {
                    new CommandOption("str", "hello world")
                })
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_ResolveCommand_IsRequired))]
        public void ResolveCommand_IsRequired_Test(CommandOptionSet commandOptionSet)
        {
            // Arrange
            var typeProvider = new TypeProvider(typeof(TestCommand));
            var optionConverter = new CommandOptionConverter();

            var resolver = new CommandResolver(typeProvider, optionConverter);

            // Act & Assert
            Assert.Throws<CommandResolveException>(() => resolver.ResolveCommand(commandOptionSet));
        }
    }
}