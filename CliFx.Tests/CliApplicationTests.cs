using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Services;
using CliFx.Tests.TestObjects;
using Moq;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public class CliApplicationTests
    {
        [Test]
        public async Task RunAsync_Test()
        {
            // Arrange
            var command = new TestCommand();
            var expectedExitCode = await command.ExecuteAsync();

            var commandResolverMock = new Mock<ICommandResolver>();
            commandResolverMock.Setup(m => m.ResolveCommand(It.IsAny<IReadOnlyList<string>>())).Returns(command);
            var commandResolver = commandResolverMock.Object;

            var application = new CliApplication(commandResolver);

            // Act
            var exitCodeValue = await application.RunAsync();

            // Assert
            Assert.That(exitCodeValue, Is.EqualTo(expectedExitCode.Value));
        }
    }
}