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
        private static ICommandSchema GetCommandSchema(Type commandType) =>
            new CommandSchemaResolver(new CommandArgumentSchemasValidator()).GetCommandSchemas(new[] {commandType}).Single();

        private static IEnumerable<TestCaseData> GetTestCases_CreateCommand()
        {
            yield return new TestCaseData(
                new Func<ICommandSchema, ICommand>(schema => (ICommand) Activator.CreateInstance(schema.Type)!),
                GetCommandSchema(typeof(HelloWorldDefaultCommand))
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_CreateCommand))]
        public void CreateCommand_Test(Func<ICommandSchema, ICommand> factoryMethod, ICommandSchema commandSchema)
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