using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Models;
using CliFx.Services;
using NUnit.Framework;

namespace CliFx.Tests
{
    public partial class CommandInitializerTests
    {
        [Command]
        public class TestCommand : ICommand
        {
            [CommandOption("int", 'i', IsRequired = true)]
            public int IntOption { get; set; } = 24;

            [CommandOption("str", 's')]
            public string StringOption { get; set; } = "foo bar";

            [CommandOption("bool", 'b', GroupName = "other-group")]
            public bool BoolOption { get; set; }

            public Task ExecuteAsync(CommandContext context) => throw new NotImplementedException();
        }
    }

    [TestFixture]
    public partial class CommandInitializerTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_InitializeCommand()
        {
            yield return new TestCaseData(
                new CommandInput(new[]
                {
                    new CommandOptionInput("int", "13")
                }),
                new TestCommand {IntOption = 13}
            );

            yield return new TestCaseData(
                new CommandInput(new[]
                {
                    new CommandOptionInput("int", "13"),
                    new CommandOptionInput("str", "hello world")
                }),
                new TestCommand {IntOption = 13, StringOption = "hello world"}
            );

            yield return new TestCaseData(
                new CommandInput(new[]
                {
                    new CommandOptionInput("i", "13")
                }),
                new TestCommand {IntOption = 13}
            );

            yield return new TestCaseData(
                new CommandInput(new[]
                {
                    new CommandOptionInput("bool")
                }),
                new TestCommand {BoolOption = true}
            );

            yield return new TestCaseData(
                new CommandInput(new[]
                {
                    new CommandOptionInput("b")
                }),
                new TestCommand {BoolOption = true}
            );

            yield return new TestCaseData(
                new CommandInput(new[]
                {
                    new CommandOptionInput("bool"),
                    new CommandOptionInput("str", "hello world")
                }),
                new TestCommand {BoolOption = true}
            );

            yield return new TestCaseData(
                new CommandInput(new[]
                {
                    new CommandOptionInput("int", "13"),
                    new CommandOptionInput("str", "hello world"),
                    new CommandOptionInput("bool")
                }),
                new TestCommand {IntOption = 13, StringOption = "hello world"}
            );
        }

        private static IEnumerable<TestCaseData> GetTestCases_InitializeCommand_IsRequired()
        {
            yield return new TestCaseData(CommandInput.Empty);

            yield return new TestCaseData(
                new CommandInput(new[]
                {
                    new CommandOptionInput("str", "hello world")
                })
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_InitializeCommand))]
        public void InitializeCommand_Test(CommandInput commandInput, TestCommand expectedCommand)
        {
            // Arrange
            var initializer = new CommandInitializer();

            // Act
            var schema = new CommandSchemaResolver().GetCommandSchema(typeof(TestCommand));
            var command = new TestCommand();
            initializer.InitializeCommand(command, schema, commandInput);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(command.StringOption, Is.EqualTo(expectedCommand.StringOption), nameof(command.StringOption));
                Assert.That(command.IntOption, Is.EqualTo(expectedCommand.IntOption), nameof(command.IntOption));
                Assert.That(command.BoolOption, Is.EqualTo(expectedCommand.BoolOption), nameof(command.BoolOption));
            });
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_InitializeCommand_IsRequired))]
        public void InitializeCommand_IsRequired_Test(CommandInput commandInput)
        {
            // Arrange
            var initializer = new CommandInitializer();

            // Act & Assert
            var schema = new CommandSchemaResolver().GetCommandSchema(typeof(TestCommand));
            var command = new TestCommand();
            Assert.Throws<MissingCommandOptionException>(() => initializer.InitializeCommand(command, schema, commandInput));
        }
    }
}