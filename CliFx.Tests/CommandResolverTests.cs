using System.Collections.Generic;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Models;
using CliFx.Services;
using NUnit.Framework;

namespace CliFx.Tests
{
    public partial class CommandResolverTests
    {
        [DefaultCommand]
        public class TestCommand : Command
        {
            [CommandOption("int", 'i', IsRequired = true)]
            public int IntOption { get; set; } = 24;

            [CommandOption("str", 's')] public string StringOption { get; set; } = "foo bar";

            public override ExitCode Execute() => new ExitCode(IntOption, StringOption);
        }
    }

    [TestFixture]
    public partial class CommandResolverTests
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
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_ResolveCommand))]
        public void ResolveCommand_Test(CommandOptionSet commandOptionSet, TestCommand expectedCommand)
        {
            // Arrange
            var resolver = new CommandResolver(new[] {typeof(TestCommand)}, new CommandOptionConverter());

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
            var resolver = new CommandResolver(new[] {typeof(TestCommand)}, new CommandOptionConverter());

            // Act & Assert
            Assert.Throws<CommandResolveException>(() => resolver.ResolveCommand(commandOptionSet));
        }
    }
}