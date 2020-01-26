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
                new[] {typeof(HelloWorldDefaultCommand)},
                new[] {"-h"},
                new Dictionary<string, string>(),
                null
            );

            yield return new TestCaseData(
                new[] {typeof(HelloWorldDefaultCommand)},
                new[] {"--help"},
                new Dictionary<string, string>(),
                null
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
                new[] {"exc", "[preview]"},
                new Dictionary<string, string>(),
                null
            );

            yield return new TestCaseData(
                new[] {typeof(ConcatCommand)},
                new[] {"concat", "[preview]", "-o", "value"},
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

        [Test]
        [TestCaseSource(nameof(GetTestCases_RunAsync))]
        public async Task RunAsync_Test(
            IReadOnlyList<Type> commandTypes,
            IReadOnlyList<string> commandLineArguments,
            IReadOnlyDictionary<string, string> environmentVariables,
            string? expectedStdOut = null)
        {
            // Arrange
            await using var stdoutStream = new StringWriter();
            var console = new VirtualConsole(stdoutStream);

            var application = new CliApplicationBuilder()
                .AddCommands(commandTypes)
                .UseVersionText(TestVersionText)
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(commandLineArguments, environmentVariables);
            var stdOut = stdoutStream.ToString().Trim();

            // Assert
            exitCode.Should().Be(0);

            if (expectedStdOut != null)
                stdOut.Should().Be(expectedStdOut);
            else
                stdOut.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_RunAsync_Negative))]
        public async Task RunAsync_Negative_Test(
            IReadOnlyList<Type> commandTypes,
            IReadOnlyList<string> commandLineArguments,
            IReadOnlyDictionary<string, string> environmentVariables,
            string? expectedStdErr = null,
            int? expectedExitCode = null)
        {
            // Arrange
            await using var stderrStream = new StringWriter();
            var console = new VirtualConsole(TextWriter.Null, stderrStream);

            var application = new CliApplicationBuilder()
                .AddCommands(commandTypes)
                .UseVersionText(TestVersionText)
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(commandLineArguments, environmentVariables);
            var stderr = stderrStream.ToString().Trim();

            // Assert
            if (expectedExitCode != null)
                exitCode.Should().Be(expectedExitCode);
            else
                exitCode.Should().NotBe(0);

            if (expectedStdErr != null)
                stderr.Should().Be(expectedStdErr);
            else
                stderr.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public async Task RunAsync_Cancellation_Test()
        {
            // Arrange
            using var cancellationTokenSource = new CancellationTokenSource();
            await using var stdoutStream = new StringWriter();
            var console = new VirtualConsole(stdoutStream, cancellationTokenSource.Token);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(CancellableCommand))
                .UseConsole(console)
                .Build();

            var commandLineArguments = new[] {"cancel"};
            var environmentVariables = new Dictionary<string, string>();

            // Act
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(0.2));
            var exitCode = await application.RunAsync(commandLineArguments, environmentVariables);
            var stdOut = stdoutStream.ToString().Trim();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Should().BeNullOrWhiteSpace();
        }
    }
}