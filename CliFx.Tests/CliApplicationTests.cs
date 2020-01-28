using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Tests.TestCommands;

namespace CliFx.Tests
{
    [TestFixture]
    public class CliApplicationTests
    {
        private const string TestAppName = "TestApp";
        private const string TestVersionText = "v1.0";

        private static IEnumerable<TestCaseData> GetTestCases_RunAsync()
        {
            yield return new TestCaseData(
                new[] {typeof(HelloWorldDefaultCommand)},
                new string[0],
                new Dictionary<string, string>(),
                "Hello world."
            );

            yield return new TestCaseData(
                new[] {typeof(ConcatCommand)},
                new[] {"concat", "-i", "foo", "-i", "bar", "-s", " "},
                new Dictionary<string, string>(),
                "foo bar"
            );

            yield return new TestCaseData(
                new[] {typeof(ConcatCommand)},
                new[] {"concat", "-i", "one", "two", "three", "-s", ", "},
                new Dictionary<string, string>(),
                "one, two, three"
            );

            yield return new TestCaseData(
                new[] {typeof(DivideCommand)},
                new[] {"div", "-D", "24", "-d", "8"},
                new Dictionary<string, string>(),
                "3"
            );

            yield return new TestCaseData(
                new[] {typeof(HelloWorldDefaultCommand)},
                new[] {"--version"},
                new Dictionary<string, string>(),
                TestVersionText
            );

            yield return new TestCaseData(
                new[] {typeof(ConcatCommand)},
                new[] {"--version"},
                new Dictionary<string, string>(),
                TestVersionText
            );

            yield return new TestCaseData(
                new[] {typeof(ConcatCommand)},
                new string[0],
                new Dictionary<string, string>(),
                null
            );

            yield return new TestCaseData(
                new[] {typeof(ConcatCommand)},
                new[] {"-h"},
                new Dictionary<string, string>(),
                null
            );

            yield return new TestCaseData(
                new[] {typeof(ConcatCommand)},
                new[] {"--help"},
                new Dictionary<string, string>(),
                null
            );

            yield return new TestCaseData(
                new[] {typeof(ConcatCommand)},
                new[] {"concat", "-h"},
                new Dictionary<string, string>(),
                null
            );

            yield return new TestCaseData(
                new[] {typeof(ExceptionCommand)},
                new[] {"exc", "-h"},
                new Dictionary<string, string>(),
                null
            );

            yield return new TestCaseData(
                new[] {typeof(CommandExceptionCommand)},
                new[] {"exc", "-h"},
                new Dictionary<string, string>(),
                null
            );

            yield return new TestCaseData(
                new[] {typeof(ConcatCommand)},
                new[] {"[preview]"},
                new Dictionary<string, string>(),
                null
            );

            yield return new TestCaseData(
                new[] {typeof(ExceptionCommand)},
                new[] {"[preview]", "exc"},
                new Dictionary<string, string>(),
                null
            );

            yield return new TestCaseData(
                new[] {typeof(ConcatCommand)},
                new[] {"[preview]", "concat", "-o", "value"},
                new Dictionary<string, string>(),
                null
            );
        }

        private static IEnumerable<TestCaseData> GetTestCases_RunAsync_Negative()
        {
            yield return new TestCaseData(
                new Type[0],
                new string[0],
                new Dictionary<string, string>(),
                null, null
            );

            yield return new TestCaseData(
                new[] {typeof(ConcatCommand)},
                new[] {"non-existing"},
                new Dictionary<string, string>(),
                null, null
            );

            yield return new TestCaseData(
                new[] {typeof(ExceptionCommand)},
                new[] {"exc"},
                new Dictionary<string, string>(),
                null, null
            );

            yield return new TestCaseData(
                new[] {typeof(CommandExceptionCommand)},
                new[] {"exc"},
                new Dictionary<string, string>(),
                null, null
            );

            yield return new TestCaseData(
                new[] {typeof(CommandExceptionCommand)},
                new[] {"exc"},
                new Dictionary<string, string>(),
                null, null
            );

            yield return new TestCaseData(
                new[] {typeof(CommandExceptionCommand)},
                new[] {"exc", "-m", "foo bar"},
                new Dictionary<string, string>(),
                "foo bar", null
            );

            yield return new TestCaseData(
                new[] {typeof(CommandExceptionCommand)},
                new[] {"exc", "-m", "foo bar", "-c", "666"},
                new Dictionary<string, string>(),
                "foo bar", 666
            );
        }

        private static IEnumerable<TestCaseData> GetTestCases_RunAsync_Help()
        {
            yield return new TestCaseData(
                new[] {typeof(HelpDefaultCommand), typeof(HelpNamedCommand), typeof(HelpSubCommand)},
                new[] {"--help"},
                new[]
                {
                    TestVersionText,
                    "Description",
                    "HelpDefaultCommand description.",
                    "Usage",
                    TestAppName, "[command]", "[options]",
                    "Options",
                    "-a|--option-a", "OptionA description.",
                    "-b|--option-b", "OptionB description.",
                    "-h|--help", "Shows help text.",
                    "--version", "Shows version information.",
                    "Commands",
                    "cmd", "HelpNamedCommand description.",
                    "You can run", "to show help on a specific command."
                }
            );

            yield return new TestCaseData(
                new[] {typeof(HelpSubCommand)},
                new[] {"--help"},
                new[]
                {
                    TestVersionText,
                    "Usage",
                    TestAppName, "[command]",
                    "Options",
                    "-h|--help", "Shows help text.",
                    "--version", "Shows version information.",
                    "Commands",
                    "cmd sub", "HelpSubCommand description.",
                    "You can run", "to show help on a specific command."
                }
            );

            yield return new TestCaseData(
                new[] {typeof(HelpDefaultCommand), typeof(HelpNamedCommand), typeof(HelpSubCommand)},
                new[] {"cmd", "--help"},
                new[]
                {
                    "Description",
                    "HelpNamedCommand description.",
                    "Usage",
                    TestAppName, "cmd", "[command]", "[options]",
                    "Options",
                    "-c|--option-c", "OptionC description.",
                    "-d|--option-d", "OptionD description.",
                    "-h|--help", "Shows help text.",
                    "Commands",
                    "sub", "HelpSubCommand description.",
                    "You can run", "to show help on a specific command."
                }
            );

            yield return new TestCaseData(
                new[] {typeof(HelpDefaultCommand), typeof(HelpNamedCommand), typeof(HelpSubCommand)},
                new[] {"cmd", "sub", "--help"},
                new[]
                {
                    "Description",
                    "HelpSubCommand description.",
                    "Usage",
                    TestAppName, "cmd sub", "[options]",
                    "Options",
                    "-e|--option-e", "OptionE description.",
                    "-h|--help", "Shows help text."
                }
            );

            yield return new TestCaseData(
                new[] {typeof(ParameterCommand)},
                new[] {"param", "cmd", "--help"},
                new[]
                {
                    "Description",
                    "Command using positional parameters",
                    "Usage",
                    TestAppName, "param cmd", "<first>", "<parameterb>", "<third list>", "[options]",
                    "Parameters",
                    "* first",
                    "* parameterb",
                    "* third list", "A list of numbers",
                    "Options",
                    "-o|--option",
                    "-h|--help", "Shows help text."
                }
            );

            yield return new TestCaseData(
                new[] {typeof(AllRequiredOptionsCommand)},
                new[] {"allrequired", "--help"},
                new[]
                {
                    "Description",
                    "AllRequiredOptionsCommand description.",
                    "Usage",
                    TestAppName, "allrequired --option-f <value> --option-g <value>"
                }
            );

            yield return new TestCaseData(
                new[] {typeof(SomeRequiredOptionsCommand)},
                new[] {"somerequired", "--help"},
                new[]
                {
                    "Description",
                    "SomeRequiredOptionsCommand description.",
                    "Usage",
                    TestAppName, "somerequired --option-f <value> [options]"
                }
            );
        }

        [TestCaseSource(nameof(GetTestCases_RunAsync))]
        public async Task RunAsync_Test(
            IReadOnlyList<Type> commandTypes,
            IReadOnlyList<string> commandLineArguments,
            IReadOnlyDictionary<string, string> environmentVariables,
            string? expectedStdOut = null)
        {
            // Arrange
            await using var stdOutStream = new StringWriter();
            var console = new VirtualConsole(stdOutStream);

            var application = new CliApplicationBuilder()
                .AddCommands(commandTypes)
                .UseTitle(TestAppName)
                .UseExecutableName(TestAppName)
                .UseVersionText(TestVersionText)
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(commandLineArguments, environmentVariables);
            var stdOut = stdOutStream.ToString().Trim();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().NotBeNullOrWhiteSpace();

            if (expectedStdOut != null)
                stdOut.Should().Be(expectedStdOut);

            Console.WriteLine(stdOut);
        }

        [TestCaseSource(nameof(GetTestCases_RunAsync_Negative))]
        public async Task RunAsync_Negative_Test(
            IReadOnlyList<Type> commandTypes,
            IReadOnlyList<string> commandLineArguments,
            IReadOnlyDictionary<string, string> environmentVariables,
            string? expectedStdErr = null,
            int? expectedExitCode = null)
        {
            // Arrange
            await using var stdErrStream = new StringWriter();
            var console = new VirtualConsole(TextWriter.Null, stdErrStream);

            var application = new CliApplicationBuilder()
                .AddCommands(commandTypes)
                .UseTitle(TestAppName)
                .UseExecutableName(TestAppName)
                .UseVersionText(TestVersionText)
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(commandLineArguments, environmentVariables);
            var stderr = stdErrStream.ToString().Trim();

            // Assert
            exitCode.Should().NotBe(0);
            stderr.Should().NotBeNullOrWhiteSpace();

            if (expectedExitCode != null)
                exitCode.Should().Be(expectedExitCode);

            if (expectedStdErr != null)
                stderr.Should().Be(expectedStdErr);

            Console.WriteLine(stderr);
        }

        [TestCaseSource(nameof(GetTestCases_RunAsync_Help))]
        public async Task RunAsync_Help_Test(
            IReadOnlyList<Type> commandTypes,
            IReadOnlyList<string> commandLineArguments,
            IReadOnlyList<string>? expectedSubstrings = null)
        {
            // Arrange
            await using var stdOutStream = new StringWriter();
            var console = new VirtualConsole(stdOutStream);

            var application = new CliApplicationBuilder()
                .AddCommands(commandTypes)
                .UseTitle(TestAppName)
                .UseExecutableName(TestAppName)
                .UseVersionText(TestVersionText)
                .UseConsole(console)
                .Build();

            var environmentVariables = new Dictionary<string, string>();

            // Act
            var exitCode = await application.RunAsync(commandLineArguments, environmentVariables);
            var stdOut = stdOutStream.ToString().Trim();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().NotBeNullOrWhiteSpace();

            if (expectedSubstrings != null)
                stdOut.Should().ContainAll(expectedSubstrings);

            Console.WriteLine(stdOut);
        }

        [Test]
        public async Task RunAsync_Cancellation_Test()
        {
            // Arrange
            using var cancellationTokenSource = new CancellationTokenSource();

            await using var stdOutStream = new StringWriter();
            await using var stdErrStream = new StringWriter();
            var console = new VirtualConsole(stdOutStream, stdErrStream, cancellationTokenSource.Token);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(CancellableCommand))
                .UseConsole(console)
                .Build();

            var commandLineArguments = new[] {"cancel"};
            var environmentVariables = new Dictionary<string, string>();

            // Act
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(0.2));
            var exitCode = await application.RunAsync(commandLineArguments, environmentVariables);
            var stdOut = stdOutStream.ToString().Trim();
            var stdErr = stdErrStream.ToString().Trim();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Should().BeNullOrWhiteSpace();
            stdErr.Should().NotBeNullOrWhiteSpace();

            Console.WriteLine(stdErr);
        }
    }
}