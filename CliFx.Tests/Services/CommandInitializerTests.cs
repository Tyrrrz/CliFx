using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Exceptions;
using CliFx.Models;
using CliFx.Services;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Services
{
    [TestFixture]
    public partial class CommandInitializerTests
    {
        private static CommandSchema GetCommandSchema(Type commandType) =>
            new CommandSchemaResolver().GetCommandSchemas(new[] {commandType}).Single();

        private static IEnumerable<TestCaseData> GetTestCases_InitializeCommand()
        {
            yield return new TestCaseData(
                new TestCommand(),
                GetCommandSchema(typeof(TestCommand)),
                new CommandInput(new[]
                {
                    new CommandOptionInput("int", "13")
                }),
                new TestCommand {Option1 = 13}
            );

            yield return new TestCaseData(
                new TestCommand(),
                GetCommandSchema(typeof(TestCommand)),
                new CommandInput(new[]
                {
                    new CommandOptionInput("int", "13"),
                    new CommandOptionInput("str", "hello world")
                }),
                new TestCommand {Option1 = 13, Option2 = "hello world"}
            );

            yield return new TestCaseData(
                new TestCommand(),
                GetCommandSchema(typeof(TestCommand)),
                new CommandInput(new[]
                {
                    new CommandOptionInput("i", "13")
                }),
                new TestCommand {Option1 = 13}
            );

            yield return new TestCaseData(
                new TestCommand(),
                GetCommandSchema(typeof(TestCommand)),
                new CommandInput(new[]
                {
                    new CommandOptionInput("i", "13"),
                    new CommandOptionInput("s", "hello world"),
                    new CommandOptionInput("S")
                }),
                new TestCommand {Option1 = 13, Option2 = "hello world", Option3 = true}
            );
        }

        private static IEnumerable<TestCaseData> GetTestCases_InitializeCommand_Negative()
        {
            yield return new TestCaseData(
                new TestCommand(),
                GetCommandSchema(typeof(TestCommand)),
                CommandInput.Empty
            );

            yield return new TestCaseData(
                new TestCommand(),
                GetCommandSchema(typeof(TestCommand)),
                new CommandInput(new[]
                {
                    new CommandOptionInput("str", "hello world")
                })
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_InitializeCommand))]
        public void InitializeCommand_Test(ICommand command, CommandSchema commandSchema, CommandInput commandInput, ICommand expectedCommand)
        {
            // Arrange
            var initializer = new CommandInitializer();

            // Act
            initializer.InitializeCommand(command, commandSchema, commandInput);

            // Assert
            command.Should().BeEquivalentTo(expectedCommand, o => o.RespectingRuntimeTypes());
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_InitializeCommand_Negative))]
        public void InitializeCommand_Negative_Test(ICommand command, CommandSchema commandSchema, CommandInput commandInput)
        {
            // Arrange
            var initializer = new CommandInitializer();

            // Act & Assert
            initializer.Invoking(i => i.InitializeCommand(command, commandSchema, commandInput))
                .Should().ThrowExactly<MissingCommandOptionInputException>();
        }
    }
}