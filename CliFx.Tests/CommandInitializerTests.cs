using System.Collections.Generic;
using CliFx.Exceptions;
using CliFx.Models;
using CliFx.Services;
using CliFx.Tests.Internal;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests
{
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
        public void InitializeCommand_Test(CommandInput commandInput, ICommand expectedCommand)
        {
            // Arrange
            var initializer = new CommandInitializer();
            var schema = new CommandSchemaResolver().GetCommandSchema(typeof(TestCommand));
            var command = new TestCommand();

            // Act
            initializer.InitializeCommand(command, schema, commandInput);

            // Assert
            command.Should().BeEquivalentTo(expectedCommand, o => o.RespectingRuntimeTypes());
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_InitializeCommand_IsRequired))]
        public void InitializeCommand_IsRequired_Test(CommandInput commandInput)
        {
            // Arrange
            var initializer = new CommandInitializer();
            var schema = new CommandSchemaResolver().GetCommandSchema(typeof(TestCommand));
            var command = new TestCommand();

            // Act & Assert
            initializer.Invoking(i => i.InitializeCommand(command, schema, commandInput))
                .Should().ThrowExactly<MissingCommandOptionInputException>();
        }
    }
}