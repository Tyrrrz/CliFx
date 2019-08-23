using System;
using System.Collections.Generic;
using CliFx.Exceptions;
using CliFx.Models;
using CliFx.Services;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Services
{
    [TestFixture]
    public partial class CommandSchemaResolverTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_GetCommandSchemas()
        {
            yield return new TestCaseData(
                new[] {typeof(NormalCommand1), typeof(NormalCommand2)},
                new[]
                {
                    new CommandSchema(typeof(NormalCommand1), "cmd", "NormalCommand1 description.",
                        new[]
                        {
                            new CommandOptionSchema(typeof(NormalCommand1).GetProperty(nameof(NormalCommand1.OptionA)),
                                "option-a", 'a', false, null),
                            new CommandOptionSchema(typeof(NormalCommand1).GetProperty(nameof(NormalCommand1.OptionB)),
                                "option-b", null, true, null)
                        }),
                    new CommandSchema(typeof(NormalCommand2), null, "NormalCommand2 description.",
                        new[]
                        {
                            new CommandOptionSchema(typeof(NormalCommand2).GetProperty(nameof(NormalCommand2.OptionC)),
                                "option-c", null, false, "OptionC description."),
                            new CommandOptionSchema(typeof(NormalCommand2).GetProperty(nameof(NormalCommand2.OptionD)),
                                "option-d", 'd', false, null)
                        })
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
                new[] {typeof(ConflictingCommand1), typeof(ConflictingCommand2)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(InvalidCommand1)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(InvalidCommand2)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(InvalidCommand3)}
            });

            yield return new TestCaseData(new object[]
            {
                new[] {typeof(InvalidCommand4)}
            });
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_GetCommandSchemas))]
        public void GetCommandSchemas_Test(IReadOnlyList<Type> commandTypes, IReadOnlyList<CommandSchema> expectedCommandSchemas)
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
                .Should().ThrowExactly<InvalidCommandSchemaException>();
        }
    }
}