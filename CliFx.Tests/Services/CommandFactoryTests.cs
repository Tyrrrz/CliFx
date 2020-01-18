using System.Collections.Generic;
using CliFx.Domain;
using CliFx.Services;
using CliFx.Tests.TestCommands;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Services
{
    [TestFixture]
    public class CommandFactoryTests
    {

        private static IEnumerable<TestCaseData> GetTestCases_CreateCommand()
        {
            yield return new TestCaseData(SchemaLogic.ResolveCommandSchema(typeof(HelloWorldDefaultCommand)));
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