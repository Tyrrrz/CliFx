using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CliFx.Services;
using CliFx.Tests.TestCommands;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public class CliApplicationTests
    {
        private const string TestVersionText = "v1.0";

        private static IEnumerable<TestCaseData> GetTestCases_RunAsync()
        {
            yield return new TestCaseData(
                new[] {typeof(EchoDefaultCommand)},
                new[] {"-m", "foo bar"},
                "foo bar"
            );

            yield return new TestCaseData(
                new[] {typeof(EchoCommand)},
                new[] {"echo", "-m", "foo bar"},
                "foo bar"
            );

            yield return new TestCaseData(
                new[] {typeof(EchoDefaultCommand)},
                new[] {"--version"},
                TestVersionText
            );

            yield return new TestCaseData(
                new[] {typeof(EchoCommand)},
                new[] {"--version"},
                TestVersionText
            );
        }

        private static IEnumerable<TestCaseData> GetTestCases_RunAsync_Smoke()
        {
            yield return new TestCaseData(
                new[] {typeof(EchoDefaultCommand)},
                new[] {"-h"}
            );

            yield return new TestCaseData(
                new[] {typeof(EchoDefaultCommand)},
                new[] {"--help"}
            );

            yield return new TestCaseData(
                new[] {typeof(EchoDefaultCommand)},
                new[] {"--version"}
            );

            yield return new TestCaseData(
                new[] {typeof(EchoCommand)},
                new string[0]
            );

            yield return new TestCaseData(
                new[] {typeof(EchoCommand)},
                new[] {"-h"}
            );

            yield return new TestCaseData(
                new[] {typeof(EchoCommand)},
                new[] {"--help"}
            );

            yield return new TestCaseData(
                new[] {typeof(EchoCommand)},
                new[] {"--version"}
            );

            yield return new TestCaseData(
                new[] {typeof(EchoCommand)},
                new[] {"echo", "-h"}
            );

            yield return new TestCaseData(
                new[] {typeof(ExceptionCommand)},
                new[] {"exc", "-h"}
            );

            yield return new TestCaseData(
                new[] {typeof(CommandExceptionCommand)},
                new[] {"exc", "-h"}
            );

            yield return new TestCaseData(
                new[] {typeof(EchoDefaultCommand)},
                new[] {"[preview]"}
            );

            yield return new TestCaseData(
                new[] {typeof(ExceptionCommand)},
                new[] {"exc", "[preview]"}
            );

            yield return new TestCaseData(
                new[] {typeof(EchoCommand)},
                new[] {"echo", "[preview]", "-o", "value"}
            );
        }

        private static IEnumerable<TestCaseData> GetTestCases_RunAsync_Negative()
        {
            yield return new TestCaseData(
                new Type[0],
                new string[0]
            );

            yield return new TestCaseData(
                new[] {typeof(EchoCommand)},
                new[] {"non-existing"}
            );

            yield return new TestCaseData(
                new[] {typeof(ExceptionCommand)},
                new[] {"exc"}
            );

            yield return new TestCaseData(
                new[] {typeof(CommandExceptionCommand)},
                new[] {"exc"}
            );

            yield return new TestCaseData(
                new[] {typeof(CommandExceptionCommand)},
                new[] {"exc", "-c", "666"}
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_RunAsync))]
        public async Task RunAsync_Test(IReadOnlyList<Type> commandTypes, IReadOnlyList<string> commandLineArguments, string expectedStdOut)
        {
            // Arrange
            using (var stdout = new StringWriter())
            {
                var console = new VirtualConsole(stdout);

                var application = new CliApplicationBuilder()
                    .AddCommands(commandTypes)
                    .UseVersionText(TestVersionText)
                    .UseConsole(console)
                    .Build();

                // Act
                var exitCode = await application.RunAsync(commandLineArguments);

                // Assert
                exitCode.Should().Be(0);
                stdout.ToString().Trim().Should().Be(expectedStdOut);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_RunAsync_Smoke))]
        public async Task RunAsync_Smoke_Test(IReadOnlyList<Type> commandTypes, IReadOnlyList<string> commandLineArguments)
        {
            // Arrange
            using (var stdout = new StringWriter())
            {
                var console = new VirtualConsole(stdout);

                var application = new CliApplicationBuilder()
                    .AddCommands(commandTypes)
                    .UseVersionText(TestVersionText)
                    .UseConsole(console)
                    .Build();

                // Act
                var exitCode = await application.RunAsync(commandLineArguments);

                // Assert
                exitCode.Should().Be(0);
                stdout.ToString().Should().NotBeNullOrWhiteSpace();
            }
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_RunAsync_Negative))]
        public async Task RunAsync_Negative_Test(IReadOnlyList<Type> commandTypes, IReadOnlyList<string> commandLineArguments)
        {
            // Arrange
            using (var stderr = new StringWriter())
            {
                var console = new VirtualConsole(TextWriter.Null, stderr);

                var application = new CliApplicationBuilder()
                    .AddCommands(commandTypes)
                    .UseVersionText(TestVersionText)
                    .UseConsole(console)
                    .Build();

                // Act
                var exitCode = await application.RunAsync(commandLineArguments);

                // Assert
                exitCode.Should().NotBe(0);
                stderr.ToString().Should().NotBeNullOrWhiteSpace();
            }
        }
    }
}