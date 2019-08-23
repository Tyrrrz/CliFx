using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Models;
using CliFx.Services;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Services
{
    [TestFixture]
    public partial class CommandFactoryTests
    {
        private static CommandSchema GetCommandSchema(Type commandType) =>
            new CommandSchemaResolver().GetCommandSchemas(new[] {commandType}).Single();

        private static IEnumerable<TestCaseData> GetTestCases_CreateCommand()
        {
            yield return new TestCaseData(GetCommandSchema(typeof(TestCommand)));
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_CreateCommand))]
        public void CreateCommand_Test(CommandSchema commandSchema)
        {
            // Arrange
            var factory = new CommandFactory();

            // Act
            var command = factory.CreateCommand(commandSchema);

            // Assert
            command.Should().BeOfType(commandSchema.Type);
        }
    }
}