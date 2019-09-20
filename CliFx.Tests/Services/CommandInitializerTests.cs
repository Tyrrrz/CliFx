using CliFx.Exceptions;
using CliFx.Models;
using CliFx.Services;
using CliFx.Tests.Stubs;
using CliFx.Tests.TestCommands;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CliFx.Tests.Services
{
    [TestFixture]
    public class CommandInitializerTests
    {
        private static CommandSchema GetCommandSchema(Type commandType) =>
            new CommandSchemaResolver().GetCommandSchemas(new[] { commandType }).Single();

        private static IEnumerable<TestCaseData> GetTestCases_InitializeCommand()
        {
            yield return new TestCaseData(
                new DivideCommand(),
                GetCommandSchema(typeof(DivideCommand)),
                new CommandInput("div", new[]
                {
                    new CommandOptionInput("dividend", "13"),
                    new CommandOptionInput("divisor", "8")
                }),
                new DivideCommand { Dividend = 13, Divisor = 8 }
            );

            yield return new TestCaseData(
                new DivideCommand(),
                GetCommandSchema(typeof(DivideCommand)),
                new CommandInput("div", new[]
                {
                    new CommandOptionInput("dividend", "13"),
                    new CommandOptionInput("d", "8")
                }),
                new DivideCommand { Dividend = 13, Divisor = 8 }
            );

            yield return new TestCaseData(
                new DivideCommand(),
                GetCommandSchema(typeof(DivideCommand)),
                new CommandInput("div", new[]
                {
                    new CommandOptionInput("D", "13"),
                    new CommandOptionInput("d", "8")
                }),
                new DivideCommand { Dividend = 13, Divisor = 8 }
            );

            yield return new TestCaseData(
                new ConcatCommand(),
                GetCommandSchema(typeof(ConcatCommand)),
                new CommandInput("concat", new[]
                {
                    new CommandOptionInput("i", new[] {"foo", " ", "bar"})
                }),
                new ConcatCommand { Inputs = new[] { "foo", " ", "bar" } }
            );

            yield return new TestCaseData(
                new ConcatCommand(),
                GetCommandSchema(typeof(ConcatCommand)),
                new CommandInput("concat", new[]
                {
                    new CommandOptionInput("i", new[] {"foo", "bar"}),
                    new CommandOptionInput("s", " ")
                }),
                new ConcatCommand { Inputs = new[] { "foo", "bar" }, Separator = " " }
            );

            //Will read a value from environment variables because none is supplied via CommandInput
            yield return new TestCaseData(
                new EnvironmentVariableCommand(),
                GetCommandSchema(typeof(EnvironmentVariableCommand)),
                new CommandInput(null, new CommandOptionInput[] { }),
                new EnvironmentVariableCommand { Option = "A" }
            );

            //Will read multiple values from environment variables because none is supplied via CommandInput
            yield return new TestCaseData(
                new EnvironmentVariableWithMultipleValuesCommand(),
                GetCommandSchema(typeof(EnvironmentVariableWithMultipleValuesCommand)),
                new CommandInput(null, new CommandOptionInput[] { }),
                new EnvironmentVariableWithMultipleValuesCommand { Option = new[] { "A", "B", "C" } }
            );

            //Will not read a value from environment variables because one is supplied via CommandInput
            yield return new TestCaseData(
                new EnvironmentVariableCommand(),
                GetCommandSchema(typeof(EnvironmentVariableCommand)),
                new CommandInput(null, new[]
                {
                    new CommandOptionInput("opt", new[] {"X"})
                }),
                new EnvironmentVariableCommand { Option = "X" }
            );
        }

        private static IEnumerable<TestCaseData> GetTestCases_InitializeCommand_Negative()
        {
            yield return new TestCaseData(
                new DivideCommand(),
                GetCommandSchema(typeof(DivideCommand)),
                new CommandInput("div")
            );

            yield return new TestCaseData(
                new DivideCommand(),
                GetCommandSchema(typeof(DivideCommand)),
                new CommandInput("div", new[]
                {
                    new CommandOptionInput("D", "13")
                })
            );

            yield return new TestCaseData(
                new ConcatCommand(),
                GetCommandSchema(typeof(ConcatCommand)),
                new CommandInput("concat")
            );

            yield return new TestCaseData(
                new ConcatCommand(),
                GetCommandSchema(typeof(ConcatCommand)),
                new CommandInput("concat", new[]
                {
                    new CommandOptionInput("s", "_")
                })
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_InitializeCommand))]
        public void InitializeCommand_Test(ICommand command, CommandSchema commandSchema, CommandInput commandInput,
            ICommand expectedCommand)
        {
            // Arrange
            var initializer = new CommandInitializer(new EnvironmentVariablesProviderStub());

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
            var initializer = new CommandInitializer(new EnvironmentVariablesProviderStub());

            // Act & Assert
            initializer.Invoking(i => i.InitializeCommand(command, commandSchema, commandInput))
                .Should().ThrowExactly<CliFxException>();
        }
    }
}