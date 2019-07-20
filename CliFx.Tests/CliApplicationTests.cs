using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Models;
using CliFx.Services;
using NUnit.Framework;

namespace CliFx.Tests
{
    public partial class CliApplicationTests
    {
        [Command]
        public class TestCommand : ICommand
        {
            public static ExitCode ExitCode { get; } = new ExitCode(13);

            public CommandContext Context { get; set; }

            public Task<ExitCode> ExecuteAsync() => Task.FromResult(ExitCode);
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
                new CommandInputParser(),
                new CommandInitializer(new CommandSchemaResolver(new[] {typeof(TestCommand)})));

            // Act
            var exitCodeValue = await application.RunAsync();

            // Assert
            Assert.That(exitCodeValue, Is.EqualTo(TestCommand.ExitCode.Value), "Exit code");
        }
    }
}