using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using CliFx.Exceptions;
using CliFx.Models;
using CliFx.Services;
using CliFx.Tests.TestCommands;

namespace CliFx.Tests.Services
{
    [TestFixture]
    public class CommandSchemaResolverTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_GetCommandSchemas()
        {
            yield return new TestCaseData(
                new[] { typeof(DivideCommand), typeof(ConcatCommand), typeof(EnvironmentVariableCommand), typeof(SampleValueOptionCommand) },
                new[]
                {
                    new CommandSchema(typeof(DivideCommand), "div", "Divide one number by another.",
                        new CommandArgumentSchema[0], new[]
                        {
                            new CommandOptionSchema(typeof(DivideCommand).GetProperty(nameof(DivideCommand.Dividend)),
                                "dividend", 'D', true, "The number to divide.", null, null),
                            new CommandOptionSchema(typeof(DivideCommand).GetProperty(nameof(DivideCommand.Divisor)),
                                "divisor", 'd', true, "The number to divide by.", null, null)
                        }),
                    new CommandSchema(typeof(ConcatCommand), "concat", "Concatenate strings.",
                        new CommandArgumentSchema[0],
                        new[]
                        {
                            new CommandOptionSchema(typeof(ConcatCommand).GetProperty(nameof(ConcatCommand.Inputs)),
                                null, 'i', true, "Input strings.", null, null),
                            new CommandOptionSchema(typeof(ConcatCommand).GetProperty(nameof(ConcatCommand.Separator)),
                                null, 's', false, "String separator.", null, null)
                        }),
                    new CommandSchema(typeof(EnvironmentVariableCommand), null, "Reads option values from environment variables.",
                        new CommandArgumentSchema[0],
                        new[]
                        {
                            new CommandOptionSchema(typeof(EnvironmentVariableCommand).GetProperty(nameof(EnvironmentVariableCommand.Option)),
                                "opt", null, false, null, "ENV_SINGLE_VALUE", null)
                        }
                    ),
                    new CommandSchema(typeof(SampleValueOptionCommand), "sampleval", "SampleValueOptionCommand description.",
                        new CommandArgumentSchema[0],
                        new []
                        {
                            new CommandOptionSchema(typeof(SampleValueOptionCommand).GetProperty(nameof(SampleValueOptionCommand.OptionF)),
                                "option-f", 'f', true, "OptionF description.", null, "option_f_value")
                        }
                    )
                }
            );

            yield return new TestCaseData(
                new[] { typeof(HelloWorldDefaultCommand) },
                new[]
                {
                    new CommandSchema(typeof(HelloWorldDefaultCommand), null, null, new CommandArgumentSchema[0], new CommandOptionSchema[0])
                }
            );
        }

        private static IEnumerable<TestCaseData> GetTestCases_GetCommandSchemas_Negative()
        {
            yield return new TestCaseData(new object[]
            {
                new Type[0]
            });

            yield return new TestCaseData(new object[]
            {
                new[] { typeof(NonImplementedCommand) }
            });

            yield return new TestCaseData(new object[]
            {
                new[] { typeof(NonAnnotatedCommand) }
            });

            yield return new TestCaseData(new object[]
            {
                new[] { typeof(DuplicateOptionNamesCommand) }
            });

            yield return new TestCaseData(new object[]
            {
                new[] { typeof(DuplicateOptionShortNamesCommand) }
            });

            yield return new TestCaseData(new object[]
            {
                new[] { typeof(ExceptionCommand), typeof(CommandExceptionCommand) }
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
            // Arrange
            var commandSchemaResolver = new CommandSchemaResolver(new CommandArgumentSchemasValidator());

            // Act
            var commandSchemas = commandSchemaResolver.GetCommandSchemas(commandTypes);

            // Assert
            commandSchemas.Should().BeEquivalentTo(expectedCommandSchemas);
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_GetCommandSchemas_Negative))]
        public void GetCommandSchemas_Negative_Test(IReadOnlyList<Type> commandTypes)
        {
            // Arrange
            var resolver = new CommandSchemaResolver(new CommandArgumentSchemasValidator());

            // Act & Assert
            resolver.Invoking(r => r.GetCommandSchemas(commandTypes))
                .Should().ThrowExactly<CliFxException>();
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_GetTargetCommandSchema_Positive))]
        public void GetTargetCommandSchema_Positive_Test(IReadOnlyList<CommandSchema> availableCommandSchemas,
            CommandInput commandInput,
            IReadOnlyList<string> expectedPositionalArguments,
            string expectedCommandSchemaName)
        {
            // Arrange
            var resolver = new CommandSchemaResolver(new CommandArgumentSchemasValidator());

            // Act
            var commandCandidate = resolver.GetTargetCommandSchema(availableCommandSchemas, commandInput);

            // Assert
            commandCandidate.Should().NotBeNull();
            commandCandidate.PositionalArgumentsInput.Should().BeEquivalentTo(expectedPositionalArguments);
            commandCandidate.Schema.Name.Should().Be(expectedCommandSchemaName);
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_GetTargetCommandSchema_Negative))]
        public void GetTargetCommandSchema_Negative_Test(IReadOnlyList<CommandSchema> availableCommandSchemas, CommandInput commandInput)
        {
            // Arrange
            var resolver = new CommandSchemaResolver(new CommandArgumentSchemasValidator());

            // Act
            var commandCandidate = resolver.GetTargetCommandSchema(availableCommandSchemas, commandInput);

            // Assert
            commandCandidate.Should().BeNull();
        }
    }
}