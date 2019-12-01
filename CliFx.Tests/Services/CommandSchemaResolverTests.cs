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
                new[] { typeof(DivideCommand), typeof(ConcatCommand), typeof(EnvironmentVariableCommand) },
                new[]
                {
                    new CommandSchema(typeof(DivideCommand), "div", "Divide one number by another.",
                        new CommandArgumentSchema[0], new[]
                        {
                            new CommandOptionSchema(typeof(DivideCommand).GetProperty(nameof(DivideCommand.Dividend)),
                                "dividend", 'D', true, "The number to divide.", null),
                            new CommandOptionSchema(typeof(DivideCommand).GetProperty(nameof(DivideCommand.Divisor)),
                                "divisor", 'd', true, "The number to divide by.", null)
                        }),
                    new CommandSchema(typeof(ConcatCommand), "concat", "Concatenate strings.",
                        new CommandArgumentSchema[0],
                        new[]
                        {
                            new CommandOptionSchema(typeof(ConcatCommand).GetProperty(nameof(ConcatCommand.Inputs)),
                                null, 'i', true, "Input strings.", null),
                            new CommandOptionSchema(typeof(ConcatCommand).GetProperty(nameof(ConcatCommand.Separator)),
                                null, 's', false, "String separator.", null)
                        }),
                    new CommandSchema(typeof(EnvironmentVariableCommand), null, "Reads option values from environment variables.",
                        new CommandArgumentSchema[0],
                        new[]
                        {
                            new CommandOptionSchema(typeof(EnvironmentVariableCommand).GetProperty(nameof(EnvironmentVariableCommand.Option)),
                                "opt", null, false, null, "ENV_SINGLE_VALUE")
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
                new[] {typeof(NonImplementedCommand)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(NonAnnotatedCommand)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(DuplicateOptionNamesCommand)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(DuplicateOptionShortNamesCommand)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(ExceptionCommand), typeof(CommandExceptionCommand)}
            });
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_GetCommandSchemas))]
        public void GetCommandSchemas_Test(IReadOnlyList<Type> commandTypes,
            IReadOnlyList<CommandSchema> expectedCommandSchemas)
        {
            // Arrange
            var commandSchemaResolver = new CommandSchemaResolver();

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
            var resolver = new CommandSchemaResolver();

            // Act & Assert
            resolver.Invoking(r => r.GetCommandSchemas(commandTypes))
                .Should().ThrowExactly<CliFxException>();
        }
    }
}