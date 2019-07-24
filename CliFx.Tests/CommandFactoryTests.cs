using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Models;
using CliFx.Services;
using NUnit.Framework;

namespace CliFx.Tests
{
    public partial class CommandFactoryTests
    {
        private class TestCommand : ICommand
        {
            public Task ExecuteAsync(CommandContext context) => throw new NotImplementedException();
        }
    }

    [TestFixture]
    public partial class CommandFactoryTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_CreateCommand()
        {
            yield return new TestCaseData(typeof(TestCommand));
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_CreateCommand))]
        public void CreateCommand_Test(Type commandType)
        {
            // Arrange
            var factory = new CommandFactory();

            // Act
            var schema = new CommandSchemaResolver().GetCommandSchema(commandType);
            var command = factory.CreateCommand(schema);

            // Assert
            Assert.That(command, Is.TypeOf(commandType));
        }
    }
}