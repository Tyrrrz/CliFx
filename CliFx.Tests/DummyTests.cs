using System.Threading.Tasks;
using CliWrap;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public class DummyTests
    {
        private static string DummyFilePath => typeof(Dummy.Program).Assembly.Location;

        private static string DummyVersionText => typeof(Dummy.Program).Assembly.GetName().Version.ToString();

        [Test]
        [TestCase("", "Hello world")]
        [TestCase("-t .NET", "Hello .NET")]
        [TestCase("-e", "Hello world!!!")]
        [TestCase("add -v 1 2", "3")]
        [TestCase("add -v 2.75 3.6 4.18", "10.53")]
        [TestCase("add -v 4 -v 16", "20")]
        [TestCase("log -v 100", "2")]
        [TestCase("log --value 256 --base 2", "8")]
        public async Task CliApplication_RunAsync_Test(string arguments, string expectedOutput)
        {
            // Act
            var result = await Cli.Wrap(DummyFilePath).SetArguments(arguments).ExecuteAsync();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.ExitCode, Is.Zero, "Exit code");
                Assert.That(result.StandardOutput.Trim(), Is.EqualTo(expectedOutput), "Stdout");
                Assert.That(result.StandardError.Trim(), Is.Empty, "Stderr");
            });
        }

        [Test]
        [TestCase("--version")]
        public async Task CliApplication_RunAsync_Version_Test(string arguments)
        {
            // Act
            var result = await Cli.Wrap(DummyFilePath).SetArguments(arguments).ExecuteAsync();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.ExitCode, Is.Zero, "Exit code");
                Assert.That(result.StandardOutput.Trim(), Is.EqualTo(DummyVersionText), "Stdout");
                Assert.That(result.StandardError.Trim(), Is.Empty, "Stderr");
            });
        }

        [Test]
        [TestCase("--help")]
        [TestCase("-h")]
        [TestCase("add -h")]
        [TestCase("add --help")]
        [TestCase("log -h")]
        [TestCase("log --help")]
        public async Task CliApplication_RunAsync_Help_Test(string arguments)
        {
            // Act
            var result = await Cli.Wrap(DummyFilePath).SetArguments(arguments).ExecuteAsync();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.ExitCode, Is.Zero, "Exit code");
                Assert.That(result.StandardOutput.Trim(), Is.Not.Empty, "Stdout");
                Assert.That(result.StandardError.Trim(), Is.Empty, "Stderr");
            });
        }
    }
}