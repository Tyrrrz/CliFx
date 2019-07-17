using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Models;
using CliFx.Services;
using NUnit.Framework;

namespace CliFx.Tests
{
    public partial class CliApplicationTests
    {
        [DefaultCommand]
        public class TestCommand : Command
        {
            public override ExitCode Execute() => new ExitCode(13);
        }
    }

    [TestFixture]
    public partial class CliApplicationTests
    {
        [Test]
        public async Task RunAsync_Test()
        {
            // Arrange
            var application = new CliApplication(
                new CommandOptionParser(),
                new CommandResolver(new[] {typeof(TestCommand)}, new CommandOptionConverter()));

            // Act
            var exitCodeValue = await application.RunAsync();

            // Assert
            Assert.That(exitCodeValue, Is.EqualTo(13), "Exit code");
        }
    }
}