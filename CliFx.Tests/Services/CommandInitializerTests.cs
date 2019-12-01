using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Exceptions;
using CliFx.Models;
using CliFx.Services;
using CliFx.Tests.TestCommands;
using CliFx.Tests.Stubs;
using System.IO;

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
                new CommandCandidate(
                    GetCommandSchema(typeof(DivideCommand)),
                    new string[0],
                    new CommandInput(new[] { "div" }, new[]
                    {
                        new CommandOptionInput("dividend", "13"),
                        new CommandOptionInput("divisor", "8")
                    })),
                new DivideCommand { Dividend = 13, Divisor = 8 }
            );

            yield return new TestCaseData(
                new DivideCommand(),
                new CommandCandidate(
                    GetCommandSchema(typeof(DivideCommand)),
                    new string[0],
                    new CommandInput(new[] { "div" }, new[]
                    {
                        new CommandOptionInput("dividend", "13"),
                        new CommandOptionInput("d", "8")
                    })),
                new DivideCommand { Dividend = 13, Divisor = 8 }
            );

            yield return new TestCaseData(
                new DivideCommand(),
                new CommandCandidate(
                    GetCommandSchema(typeof(DivideCommand)),
                    new string[0],
                    new CommandInput(new[] { "div" }, new[]
                    {
                        new CommandOptionInput("D", "13"),
                        new CommandOptionInput("d", "8")
                    })),
                new DivideCommand { Dividend = 13, Divisor = 8 }
            );

            yield return new TestCaseData(
                new ConcatCommand(),
                new CommandCandidate(
                    GetCommandSchema(typeof(ConcatCommand)),
                    new string[0],
                    new CommandInput(new[] { "concat" }, new[]
                    {
                        new CommandOptionInput("i", new[] { "foo", " ", "bar" })
                    })),
                new ConcatCommand { Inputs = new[] { "foo", " ", "bar" } }
            );

            yield return new TestCaseData(
                new ConcatCommand(),
                new CommandCandidate(
                    GetCommandSchema(typeof(ConcatCommand)),
                    new string[0],
                    new CommandInput(new[] { "concat" }, new[]
                    {
                        new CommandOptionInput("i", new[] { "foo", "bar" }),
                        new CommandOptionInput("s", " ")
                    })),
                new ConcatCommand { Inputs = new[] { "foo", "bar" }, Separator = " " }
            );

            //Will read a value from environment variables because none is supplied via CommandInput
            yield return new TestCaseData(
                new EnvironmentVariableCommand(),
                new CommandCandidate(
                    GetCommandSchema(typeof(EnvironmentVariableCommand)),
                    new string[0],
                    new CommandInput(new string[0], new CommandOptionInput[0], EnvironmentVariablesProviderStub.EnvironmentVariables)),
                new EnvironmentVariableCommand { Option = "A" }
            );

            //Will read multiple values from environment variables because none is supplied via CommandInput
            yield return new TestCaseData(
                new EnvironmentVariableWithMultipleValuesCommand(),
                new CommandCandidate(
                    GetCommandSchema(typeof(EnvironmentVariableWithMultipleValuesCommand)),
                    new string[0],
                    new CommandInput(new string[0], new CommandOptionInput[0], EnvironmentVariablesProviderStub.EnvironmentVariables)),
                new EnvironmentVariableWithMultipleValuesCommand { Option = new[] { "A", "B", "C" } }
            );

            //Will not read a value from environment variables because one is supplied via CommandInput
            yield return new TestCaseData(
                new EnvironmentVariableCommand(),
                new CommandCandidate(
                    GetCommandSchema(typeof(EnvironmentVariableCommand)),
                    new string[0],
                    new CommandInput(new string[0], new[]
                        {
                            new CommandOptionInput("opt", new[] { "X" })
                        },
                        EnvironmentVariablesProviderStub.EnvironmentVariables)),
                new EnvironmentVariableCommand { Option = "X" }
            );

            //Will not split environment variable values because underlying property is not a collection
            yield return new TestCaseData(
                new EnvironmentVariableWithoutCollectionPropertyCommand(),
                new CommandCandidate(
                    GetCommandSchema(typeof(EnvironmentVariableWithoutCollectionPropertyCommand)),
                    new string[0],
                    new CommandInput(new string[0], new CommandOptionInput[0], EnvironmentVariablesProviderStub.EnvironmentVariables)),
                    new EnvironmentVariableWithoutCollectionPropertyCommand { Option = $"A{Path.PathSeparator}B{Path.PathSeparator}C{Path.PathSeparator}" }
                );
        }

        private static IEnumerable<TestCaseData> GetTestCases_InitializeCommand_Negative()
        {
            yield return new TestCaseData(
                new DivideCommand(),
                new CommandCandidate(
                    GetCommandSchema(typeof(DivideCommand)),
                    new string[0],
                    new CommandInput(new[] { "div" })
                ));

            yield return new TestCaseData(
                new DivideCommand(),
                new CommandCandidate(
                    GetCommandSchema(typeof(DivideCommand)),
                    new string[0],
                    new CommandInput(new[] { "div" }, new[]
                    {
                        new CommandOptionInput("D", "13")
                    })
                ));

            yield return new TestCaseData(
                new ConcatCommand(),
                    new CommandCandidate(
                    GetCommandSchema(typeof(ConcatCommand)),
                    new string[0],
                    new CommandInput(new[] { "concat" })
                ));

            yield return new TestCaseData(
                new ConcatCommand(),
                new CommandCandidate(
                    GetCommandSchema(typeof(ConcatCommand)),
                    new string[0],
                    new CommandInput(new[] { "concat" }, new[]
                    {
                        new CommandOptionInput("s", "_")
                    })
                ));
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_InitializeCommand))]
        public void InitializeCommand_Test(ICommand command, CommandCandidate commandCandidate,
            ICommand expectedCommand)
        {
            // Arrange
            var initializer = new CommandInitializer();

            // Act
            initializer.InitializeCommand(command, commandCandidate);

            // Assert
            command.Should().BeEquivalentTo(expectedCommand, o => o.RespectingRuntimeTypes());
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_InitializeCommand_Negative))]
        public void InitializeCommand_Negative_Test(ICommand command, CommandCandidate commandCandidate)
        {
            // Arrange
            var initializer = new CommandInitializer();

            // Act & Assert
            initializer.Invoking(i => i.InitializeCommand(command, commandCandidate))
                .Should().ThrowExactly<CliFxException>();
        }
    }
}