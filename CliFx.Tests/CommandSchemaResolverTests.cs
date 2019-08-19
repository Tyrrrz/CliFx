using System;
using System.Collections.Generic;
using CliFx.Models;
using CliFx.Services;
using CliFx.Tests.Internal;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public partial class CommandSchemaResolverTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_ResolveAllSchemas()
        {
            yield return new TestCaseData(
                typeof(TestCommand),
                new CommandSchema(typeof(TestCommand), "Command name", "Command description",
                    new[]
                    {
                        new CommandOptionSchema(typeof(TestCommand).GetProperty(nameof(TestCommand.OptionA)),
                            "option-a", 'a', false, null),
                        new CommandOptionSchema(typeof(TestCommand).GetProperty(nameof(TestCommand.OptionB)),
                            "option-b", null, true, null),
                        new CommandOptionSchema(typeof(TestCommand).GetProperty(nameof(TestCommand.OptionC)),
                            "option-c", null, false, "Option C description")
                    })
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_ResolveAllSchemas))]
        public void GetCommandSchema_Test(Type commandType, CommandSchema expectedSchema)
        {
            // Arrange
            var resolver = new CommandSchemaResolver();

            // Act
            var schema = resolver.GetCommandSchema(commandType);

            // Assert
            schema.Should().BeEquivalentTo(expectedSchema);
        }
    }
}