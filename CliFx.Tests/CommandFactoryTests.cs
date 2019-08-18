using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Models;
using CliFx.Services;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests
{
    public partial class CommandFactoryTests
    {
        [Command]
        private class TestCommand : ICommand
        {
            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
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
            var command = factory.CreateCommand(new CommandSchema(commandType, null, null, new CommandOptionSchema[0]));

            // Assert
            command.Should().BeOfType(commandType);
        }
    }
}