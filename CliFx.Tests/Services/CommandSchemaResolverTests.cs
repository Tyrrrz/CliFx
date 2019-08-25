using System;
using System.Collections.Generic;
using CliFx.Exceptions;
using CliFx.Models;
using CliFx.Services;
using CliFx.Tests.TestCommands;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Services
{
    [TestFixture]
    public class CommandSchemaResolverTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_GetCommandSchemas()
        {
            yield return new TestCaseData(
                new[] {typeof(DivideCommand), typeof(ConcatCommand)},
                new[]
                {
                    new CommandSchema(typeof(DivideCommand), "div", "Divide one number by another.",
                        new[]
                        {
                            new CommandOptionSchema(typeof(DivideCommand).GetProperty(nameof(DivideCommand.Dividend)),
                                "dividend", 'D', true, "The number to divide."),
                            new CommandOptionSchema(typeof(DivideCommand).GetProperty(nameof(DivideCommand.Divisor)),
                                "divisor", 'd', true, "The number to divide by.")
                        }),
                    new CommandSchema(typeof(ConcatCommand), "concat", "Concatenate strings.",
                        new[]
                        {
                            new CommandOptionSchema(typeof(ConcatCommand).GetProperty(nameof(ConcatCommand.Inputs)),
                                null, 'i', true, "Input strings."),
                            new CommandOptionSchema(typeof(ConcatCommand).GetProperty(nameof(ConcatCommand.Separator)),
                                null, 's', false, "String separator.")
                        })
                }
            );

            yield return new TestCaseData(
                new[] {typeof(HelloWorldDefaultCommand)},
                new[]
                {
                    new CommandSchema(typeof(HelloWorldDefaultCommand), null, null, new CommandOptionSchema[0])
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
                .Should().ThrowExactly<SchemaValidationException>();
        }
    }
}