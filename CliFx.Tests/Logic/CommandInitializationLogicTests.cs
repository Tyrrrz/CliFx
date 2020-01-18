using System.Collections.Generic;
using System.IO;
using CliFx.Domain;
using CliFx.Exceptions;
using CliFx.Logic;
using CliFx.Models;
using CliFx.Tests.TestCommands;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Logic
{
    [TestFixture]
    public class CommandInitializationLogicTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_InitializeCommand()
        {
            yield return new TestCaseData(
                new DivideCommand(),
                new CommandCandidate(
                    SchemaLogic.ResolveCommandSchema(typeof(DivideCommand)),
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
                    SchemaLogic.ResolveCommandSchema(typeof(DivideCommand)),
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
                    SchemaLogic.ResolveCommandSchema(typeof(DivideCommand)),
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
                    SchemaLogic.ResolveCommandSchema(typeof(ConcatCommand)),
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
                    SchemaLogic.ResolveCommandSchema(typeof(ConcatCommand)),
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
                    SchemaLogic.ResolveCommandSchema(typeof(EnvironmentVariableCommand)),
                    new string[0],
                    new CommandInput(new string[0], new CommandOptionInput[0], new Dictionary<string, string>
                    {
                        ["ENV_SINGLE_VALUE"] = "A"
                    })),
                new EnvironmentVariableCommand { Option = "A" }
            );

            //Will read multiple values from environment variables because none is supplied via CommandInput
            yield return new TestCaseData(
                new EnvironmentVariableWithMultipleValuesCommand(),
                new CommandCandidate(
                    SchemaLogic.ResolveCommandSchema(typeof(EnvironmentVariableWithMultipleValuesCommand)),
                    new string[0],
                    new CommandInput(new string[0], new CommandOptionInput[0], new Dictionary<string, string>
                    {
                        ["ENV_MULTIPLE_VALUES"] = $"A{Path.PathSeparator}B{Path.PathSeparator}C{Path.PathSeparator}"
                    })),
                new EnvironmentVariableWithMultipleValuesCommand { Option = new[] { "A", "B", "C" } }
            );

            //Will not read a value from environment variables because one is supplied via CommandInput
            yield return new TestCaseData(
                new EnvironmentVariableCommand(),
                new CommandCandidate(
                    SchemaLogic.ResolveCommandSchema(typeof(EnvironmentVariableCommand)),
                    new string[0],
                    new CommandInput(new string[0], new[]
                        {
                            new CommandOptionInput("opt", new[] { "X" })
                        },
                        new Dictionary<string, string>
                        {
                            ["ENV_SINGLE_VALUE"] = "foo"
                        })),
                new EnvironmentVariableCommand { Option = "X" }
            );

            //Will not split environment variable values because underlying property is not a collection
            yield return new TestCaseData(
                new EnvironmentVariableWithoutCollectionPropertyCommand(),
                new CommandCandidate(
                    SchemaLogic.ResolveCommandSchema(typeof(EnvironmentVariableWithoutCollectionPropertyCommand)),
                    new string[0],
                    new CommandInput(new string[0], new CommandOptionInput[0], new Dictionary<string, string>
                    {
                        ["ENV_MULTIPLE_VALUES"] = $"A{Path.PathSeparator}B{Path.PathSeparator}C{Path.PathSeparator}"
                    })),
                    new EnvironmentVariableWithoutCollectionPropertyCommand { Option = $"A{Path.PathSeparator}B{Path.PathSeparator}C{Path.PathSeparator}" }
                );

            // Positional arguments
            yield return new TestCaseData(
                new ArgumentCommand(),
                new CommandCandidate(
                    SchemaLogic.ResolveCommandSchema(typeof(ArgumentCommand)),
                    new [] { "abc", "123", "1", "2" },
                    new CommandInput(new [] { "arg", "cmd", "abc", "123", "1", "2" }, new []{ new CommandOptionInput("o", "option value") }, new Dictionary<string, string>())),
                new ArgumentCommand { FirstArgument = "abc", SecondArgument = 123, ThirdArguments = new List<int>{1, 2}, Option = "option value" }
                );
        }

        private static IEnumerable<TestCaseData> GetTestCases_InitializeCommand_Negative()
        {
            yield return new TestCaseData(
                new DivideCommand(),
                new CommandCandidate(
                    SchemaLogic.ResolveCommandSchema(typeof(DivideCommand)),
                    new string[0],
                    new CommandInput(new[] { "div" })
                ));

            yield return new TestCaseData(
                new DivideCommand(),
                new CommandCandidate(
                    SchemaLogic.ResolveCommandSchema(typeof(DivideCommand)),
                    new string[0],
                    new CommandInput(new[] { "div" }, new[]
                    {
                        new CommandOptionInput("D", "13")
                    })
                ));

            yield return new TestCaseData(
                new ConcatCommand(),
                    new CommandCandidate(
                    SchemaLogic.ResolveCommandSchema(typeof(ConcatCommand)),
                    new string[0],
                    new CommandInput(new[] { "concat" })
                ));

            yield return new TestCaseData(
                new ConcatCommand(),
                new CommandCandidate(
                    SchemaLogic.ResolveCommandSchema(typeof(ConcatCommand)),
                    new string[0],
                    new CommandInput(new[] { "concat" }, new[]
                    {
                        new CommandOptionInput("s", "_")
                    })
                ));

            // Missing required positional argument
            yield return new TestCaseData(
                new ArgumentCommand(),
                new CommandCandidate(
                    SchemaLogic.ResolveCommandSchema(typeof(ArgumentCommand)),
                    new string[0],
                    new CommandInput(new string[0], new []{ new CommandOptionInput("o", "option value") }, new Dictionary<string, string>()))
                );

            // Incorrect data type in list
            yield return new TestCaseData(
                new ArgumentCommand(),
                new CommandCandidate(
                    SchemaLogic.ResolveCommandSchema(typeof(ArgumentCommand)),
                    new []{ "abc", "123", "invalid" },
                    new CommandInput(new [] { "arg", "cmd", "abc", "123", "invalid" }, new []{ new CommandOptionInput("o", "option value") }, new Dictionary<string, string>()))
                );

            // Extraneous unused arguments
            yield return new TestCaseData(
                new SimpleArgumentCommand(),
                new CommandCandidate(
                    SchemaLogic.ResolveCommandSchema(typeof(SimpleArgumentCommand)),
                    new []{ "abc", "123", "unused" },
                    new CommandInput(new [] { "arg", "cmd2", "abc", "123", "unused" }, new []{ new CommandOptionInput("o", "option value") }, new Dictionary<string, string>()))
                );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_InitializeCommand))]
        public void InitializeCommand_Test(ICommand command, CommandCandidate commandCandidate,
            ICommand expectedCommand)
        {
            // Act
            CommandInitializationLogic.InitializeCommand(command, commandCandidate);

            // Assert
            command.Should().BeEquivalentTo(expectedCommand, o => o.RespectingRuntimeTypes());
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_InitializeCommand_Negative))]
        public void InitializeCommand_Negative_Test(ICommand command, CommandCandidate commandCandidate)
        {
            // Act & Assert
            Assert.Throws<CliFxException>(() => CommandInitializationLogic.InitializeCommand(command, commandCandidate));
        }
    }
}