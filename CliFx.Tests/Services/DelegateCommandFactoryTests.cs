using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Models;
using CliFx.Services;
using CliFx.Tests.TestCommands;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests.Services
{
    [TestFixture]
    public class DelegateCommandFactoryTests
    {
        private static CommandSchema GetCommandSchema(Type commandType) =>
            new CommandSchemaResolver().GetCommandSchemas(new[] {commandType}).Single();

        private static IEnumerable<TestCaseData> GetTestCases_CreateCommand()
        {
            yield return new TestCaseData(
                new Func<CommandSchema, ICommand>(schema => (ICommand) Activator.CreateInstance(schema.Type!)!),
                GetCommandSchema(typeof(HelloWorldDefaultCommand))
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_CreateCommand))]
        public void CreateCommand_Test(Func<CommandSchema, ICommand> factoryMethod, CommandSchema commandSchema)
        {
            // Arrange
            var factory = new DelegateCommandFactory(factoryMethod);

            // Act
            var command = factory.CreateCommand(commandSchema);

            // Assert
            command.Should().BeOfType(commandSchema.Type);
        }
    }
}