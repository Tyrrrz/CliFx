using System.IO;
using System.Threading.Tasks;
using CliWrap;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public class DummyTests
    {
        private static string DummyFilePath => Path.Combine(TestContext.CurrentContext.TestDirectory, "CliFx.Tests.Dummy.exe");

        [Test]
        [TestCase("", "Hello world")]
        [TestCase("-t .NET", "Hello .NET")]
        [TestCase("-e", "Hello world!!!")]
        [TestCase("add --a 1 --b 2", "3")]
        [TestCase("add --a 2.75 --b 3.6", "6.35")]
        [TestCase("log --value 100", "2")]
        [TestCase("log --value 256 --base 2", "8")]
        public async Task Execute_Test(string arguments, string expectedOutput)
        {
            // Act
            var result = await Cli.Wrap(DummyFilePath).SetArguments(arguments).ExecuteAsync();

            // Assert
            Assert.That(result.ExitCode, Is.Zero, nameof(result.ExitCode));
            Assert.That(result.StandardOutput.Trim(), Is.EqualTo(expectedOutput), nameof(result.StandardOutput));
            Assert.That(result.StandardError.Trim(), Is.Empty, nameof(result.StandardError));
        }
    }
}