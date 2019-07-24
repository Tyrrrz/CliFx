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
        [TestCase("-e", "Hello world!")]
        [TestCase("sum -v 1 2", "3")]
        [TestCase("sum -v 2.75 3.6 4.18", "10.53")]
        [TestCase("sum -v 4 -v 16", "20")]
        [TestCase("sum --values 2 5 --values 3", "10")]
        [TestCase("log -v 100", "2")]
        [TestCase("log --value 256 --base 2", "8")]
        public async Task CliApplication_RunAsync_Test(string arguments, string expectedOutput)
        {
            // Arrange & Act
            var result = await Cli.Wrap(DummyFilePath)
                .SetArguments(arguments)
                .EnableExitCodeValidation()
                .EnableStandardErrorValidation()
                .ExecuteAsync();

            // Assert
            Assert.That(result.StandardOutput.Trim(), Is.EqualTo(expectedOutput), "Stdout");
        }

        [Test]
        [TestCase("--version")]
        public async Task CliApplication_RunAsync_ShowVersion_Test(string arguments)
        {
            // Arrange & Act
            var result = await Cli.Wrap(DummyFilePath)
                .SetArguments(arguments)
                .EnableExitCodeValidation()
                .EnableStandardErrorValidation()
                .ExecuteAsync();

            // Assert
            Assert.That(result.StandardOutput.Trim(), Is.EqualTo(DummyVersionText), "Stdout");
        }

        [Test]
        [TestCase("--help")]
        [TestCase("-h")]
        [TestCase("sum -h")]
        [TestCase("sum --help")]
        [TestCase("log -h")]
        [TestCase("log --help")]
        public async Task CliApplication_RunAsync_ShowHelp_Test(string arguments)
        {
            // Arrange & Act
            var result = await Cli.Wrap(DummyFilePath)
                .SetArguments(arguments)
                .EnableExitCodeValidation()
                .EnableStandardErrorValidation()
                .ExecuteAsync();

            // Assert
            Assert.That(result.StandardOutput.Trim(), Is.Not.Empty, "Stdout");
        }
    }
}