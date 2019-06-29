using System.Threading.Tasks;
using CliFx.Services;
using CliFx.Tests.TestObjects;
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

            var commandOptionParser = new CommandOptionParser();

            var typeProvider = new TypeProvider(typeof(TestCommand));
            var commandOptionConverter = new CommandOptionConverter();
            var commandResolver = new CommandResolver(typeProvider, commandOptionConverter);

            var application = new CliApplication(commandOptionParser, commandResolver);

            // Act
            var exitCodeValue = await application.RunAsync();

            // Assert
            Assert.That(exitCodeValue, Is.EqualTo(expectedExitCode.Value), "Exit code");
        }
    }
}