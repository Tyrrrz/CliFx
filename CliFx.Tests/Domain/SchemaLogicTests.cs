using System;
using System.Collections.Generic;
using CliFx.Domain;
using CliFx.Exceptions;
using CliFx.Tests.TestCommands;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Domain
{
    [TestFixture]
    public class SchemaLogicTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_GetCommandSchemas()
        {
            yield return new TestCaseData(
                new[] { typeof(DivideCommand), typeof(ConcatCommand), typeof(EnvironmentVariableCommand) },
                new[]
                {
                    new CommandSchema(typeof(DivideCommand), "div", "Divide one number by another.",
                        new CommandParameterSchema[0], new[]
                        {
                            new CommandOptionSchema(typeof(DivideCommand).GetProperty(nameof(DivideCommand.Dividend)),
                                "dividend", 'D', null, true, "The number to divide."),
                            new CommandOptionSchema(typeof(DivideCommand).GetProperty(nameof(DivideCommand.Divisor)),
                                "divisor", 'd', null, true, "The number to divide by.")
                        }),
                    new CommandSchema(typeof(ConcatCommand), "concat", "Concatenate strings.",
                        new CommandParameterSchema[0],
                        new[]
                        {
                            new CommandOptionSchema(typeof(ConcatCommand).GetProperty(nameof(ConcatCommand.Inputs)),
                                null, 'i', null, true, "Input strings."),
                            new CommandOptionSchema(typeof(ConcatCommand).GetProperty(nameof(ConcatCommand.Separator)),
                                null, 's', null, false, "String separator.")
                        }),
                    new CommandSchema(typeof(EnvironmentVariableCommand), null, "Reads option values from environment variables.",
                        new CommandParameterSchema[0],
                        new[]
                        {
                            new CommandOptionSchema(typeof(EnvironmentVariableCommand).GetProperty(nameof(EnvironmentVariableCommand.Option)),
                                "opt", null, "ENV_SINGLE_VALUE", false, null)
                        }
                    )
                }
            );

            yield return new TestCaseData(
                new[] { typeof(HelloWorldDefaultCommand) },
                new[]
                {
                    new CommandSchema(typeof(HelloWorldDefaultCommand), null, null, new CommandParameterSchema[0], new CommandOptionSchema[0])
                }
            );
        }

        private static IEnumerable<TestCaseData> GetTestCases_GetCommandSchemas_Negative()
        {
            yield return new TestCaseData(new object[]
            {
                new Type[0]
            });

            // Command validation failure

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(NonImplementedCommand)}
            });

            yield return new TestCaseData(new object[]
            {
                // Same name
                new[] {typeof(ExceptionCommand), typeof(CommandExceptionCommand)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(NonAnnotatedCommand)}
            });

            // Argument validation failure

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(DuplicateArgumentOrderCommand)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(DuplicateArgumentOrderCommand)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(MultipleEnumerableArgumentsCommand)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(NonLastEnumerableArgumentCommand)}
            });

            // Option validation failure

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(DuplicateOptionNamesCommand)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(DuplicateOptionShortNamesCommand)}
            });
        }

        private static IEnumerable<TestCaseData> GetTestCases_GetTargetCommandSchema_Positive()
        {
            yield return new TestCaseData(
                new []
                {
                    new CommandSchema(null, "command1", null, null, null),
                    new CommandSchema(null, "command2", null, null, null),
                    new CommandSchema(null, "command3", null, null, null)
                },
                new CommandInput(new[] { "command1", "argument1", "argument2" }),
                new[] { "argument1", "argument2" },
                "command1"
            );
            yield return new TestCaseData(
                new []
                {
                    new CommandSchema(null, "", null, null, null),
                    new CommandSchema(null, "command1", null, null, null),
                    new CommandSchema(null, "command2", null, null, null),
                    new CommandSchema(null, "command3", null, null, null)
                },
                new CommandInput(new[] { "argument1", "argument2" }),
                new[] { "argument1", "argument2" },
                ""
            );
            yield return new TestCaseData(
                new []
                {
                    new CommandSchema(null, "command1 subcommand1", null, null, null),
                },
                new CommandInput(new[] { "command1", "subcommand1", "argument1" }),
                new[] { "argument1" },
                "command1 subcommand1"
            );
            yield return new TestCaseData(
                new []
                {
                    new CommandSchema(null, "", null, null, null),
                    new CommandSchema(null, "a", null, null, null),
                    new CommandSchema(null, "a b", null, null, null),
                    new CommandSchema(null, "a b c", null, null, null),
                    new CommandSchema(null, "b", null, null, null),
                    new CommandSchema(null, "b c", null, null, null),
                    new CommandSchema(null, "c", null, null, null),
                },
                new CommandInput(new[] { "a", "b", "d" }),
                new[] { "d" },
                "a b"
            );
            yield return new TestCaseData(
                new []
                {
                    new CommandSchema(null, "", null, null, null),
                    new CommandSchema(null, "a", null, null, null),
                    new CommandSchema(null, "a b", null, null, null),
                    new CommandSchema(null, "a b c", null, null, null),
                    new CommandSchema(null, "b", null, null, null),
                    new CommandSchema(null, "b c", null, null, null),
                    new CommandSchema(null, "c", null, null, null),
                },
                new CommandInput(new[] { "a", "b", "c", "d" }),
                new[] { "d" },
                "a b c"
            );
            yield return new TestCaseData(
                new []
                {
                    new CommandSchema(null, "", null, null, null),
                    new CommandSchema(null, "a", null, null, null),
                    new CommandSchema(null, "a b", null, null, null),
                    new CommandSchema(null, "a b c", null, null, null),
                    new CommandSchema(null, "b", null, null, null),
                    new CommandSchema(null, "b c", null, null, null),
                    new CommandSchema(null, "c", null, null, null),
                },
                new CommandInput(new[] { "b", "c" }),
                new string[0],
                "b c"
            );
            yield return new TestCaseData(
                new []
                {
                    new CommandSchema(null, "", null, null, null),
                    new CommandSchema(null, "a", null, null, null),
                    new CommandSchema(null, "a b", null, null, null),
                    new CommandSchema(null, "a b c", null, null, null),
                    new CommandSchema(null, "b", null, null, null),
                    new CommandSchema(null, "b c", null, null, null),
                    new CommandSchema(null, "c", null, null, null),
                },
                new CommandInput(new[] { "d", "a", "b"}),
                new[] { "d", "a", "b" },
                ""
            );
            yield return new TestCaseData(
                new []
                {
                    new CommandSchema(null, "", null, null, null),
                    new CommandSchema(null, "a", null, null, null),
                    new CommandSchema(null, "a b", null, null, null),
                    new CommandSchema(null, "a b c", null, null, null),
                    new CommandSchema(null, "b", null, null, null),
                    new CommandSchema(null, "b c", null, null, null),
                    new CommandSchema(null, "c", null, null, null),
                },
                new CommandInput(new[] { "a", "b c", "d" }),
                new[] { "b c", "d" },
                "a"
            );
            yield return new TestCaseData(
                new []
                {
                    new CommandSchema(null, "", null, null, null),
                    new CommandSchema(null, "a", null, null, null),
                    new CommandSchema(null, "a b", null, null, null),
                    new CommandSchema(null, "a b c", null, null, null),
                    new CommandSchema(null, "b", null, null, null),
                    new CommandSchema(null, "b c", null, null, null),
                    new CommandSchema(null, "c", null, null, null),
                },
                new CommandInput(new[] { "a b", "c", "d" }),
                new[] { "a b", "c", "d" },
                ""
            );
        }

        private static IEnumerable<TestCaseData> GetTestCases_GetTargetCommandSchema_Negative()
        {
            yield return new TestCaseData(
                new []
                {
                    new CommandSchema(null, "command1", null, null, null),
                    new CommandSchema(null, "command2", null, null, null),
                    new CommandSchema(null, "command3", null, null, null),
                },
                new CommandInput(new[] { "command4", "argument1" })
            );
            yield return new TestCaseData(
                new []
                {
                    new CommandSchema(null, "command1", null, null, null),
                    new CommandSchema(null, "command2", null, null, null),
                    new CommandSchema(null, "command3", null, null, null),
                },
                new CommandInput(new[] { "argument1" })
            );
            yield return new TestCaseData(
                new []
                {
                    new CommandSchema(null, "command1 subcommand1", null, null, null),
                },
                new CommandInput(new[] { "command1", "argument1" })
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_GetCommandSchemas))]
        public void GetCommandSchemas_Test(IReadOnlyList<Type> commandTypes,
            IReadOnlyList<CommandSchema> expectedCommandSchemas)
        {
            // Act
            var commandSchemas = SchemaLogic.ResolveCommandSchemas(commandTypes);

            // Assert
            commandSchemas.Should().BeEquivalentTo(expectedCommandSchemas);
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_GetCommandSchemas_Negative))]
        public void GetCommandSchemas_Negative_Test(IReadOnlyList<Type> commandTypes)
        {
            // Act & Assert
            Assert.Throws<CliFxException>(() => SchemaLogic.ResolveCommandSchemas(commandTypes));
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_GetTargetCommandSchema_Positive))]
        public void GetTargetCommandSchema_Positive_Test(IReadOnlyList<CommandSchema> availableCommandSchemas,
            CommandInput commandInput,
            IReadOnlyList<string> expectedPositionalArguments,
            string expectedCommandSchemaName)
        {
            // Act
            var commandCandidate = SchemaLogic.GetTargetCommandSchema(availableCommandSchemas, commandInput);

            // Assert
            commandCandidate.Should().NotBeNull();
            commandCandidate.PositionalArgumentsInput.Should().BeEquivalentTo(expectedPositionalArguments);
            commandCandidate.Schema.Name.Should().Be(expectedCommandSchemaName);
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_GetTargetCommandSchema_Negative))]
        public void GetTargetCommandSchema_Negative_Test(IReadOnlyList<CommandSchema> availableCommandSchemas, CommandInput commandInput)
        {
            // Act
            var commandCandidate = SchemaLogic.GetTargetCommandSchema(availableCommandSchemas, commandInput);

            // Assert
            commandCandidate.Should().BeNull();
        }
    }
}