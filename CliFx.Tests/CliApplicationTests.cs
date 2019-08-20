using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CliFx.Services;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public partial class CliApplicationTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_RunAsync()
        {
            yield return new TestCaseData(
                new[] {typeof(DefaultCommand)},
                new string[0],
                "DefaultCommand executed."
            );

            yield return new TestCaseData(
                new[] {typeof(NamedCommand)},
                new[] {"cmd"},
                "NamedCommand executed."
            );
        }

        private static IEnumerable<TestCaseData> GetTestCases_HelpAndVersion_RunAsync()
        {
            yield return new TestCaseData(
                new[] {typeof(DefaultCommand)},
                new string[0]
            );

            yield return new TestCaseData(
                new[] {typeof(DefaultCommand)},
                new[] {"-h"}
            );

            yield return new TestCaseData(
                new[] {typeof(DefaultCommand)},
                new[] {"--help"}
            );

            yield return new TestCaseData(
                new[] {typeof(DefaultCommand)},
                new[] {"--version"}
            );

            yield return new TestCaseData(
                new[] {typeof(NamedCommand)},
                new string[0]
            );

            yield return new TestCaseData(
                new[] {typeof(NamedCommand)},
                new[] {"cmd", "-h"}
            );

            yield return new TestCaseData(
                new[] {typeof(FaultyCommand1)},
                new[] {"faulty1", "-h"}
            );

            yield return new TestCaseData(
                new[] {typeof(FaultyCommand2)},
                new[] {"faulty2", "-h"}
            );

            yield return new TestCaseData(
                new[] {typeof(FaultyCommand3)},
                new[] {"faulty3", "-h"}
            );
        }

        private static IEnumerable<TestCaseData> GetTestCases_RunAsync_Negative()
        {
            yield return new TestCaseData(
                new Type[0],
                new string[0]
            );

            yield return new TestCaseData(
                new[] {typeof(DefaultCommand)},
                new[] {"non-existing"}
            );

            yield return new TestCaseData(
                new[] {typeof(FaultyCommand1)},
                new[] {"faulty1"}
            );

            yield return new TestCaseData(
                new[] {typeof(FaultyCommand2)},
                new[] {"faulty2"}
            );

            yield return new TestCaseData(
                new[] {typeof(FaultyCommand3)},
                new[] {"faulty3"}
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
        [TestCaseSource(nameof(GetTestCases_HelpAndVersion_RunAsync))]
        public async Task RunAsync_HelpAndVersion_Test(IReadOnlyList<Type> commandTypes, IReadOnlyList<string> commandLineArguments)
        {
            // Arrange
            using (var stdout = new StringWriter())
            {
                var console = new VirtualConsole(stdout);

                var application = new CliApplicationBuilder()
                    .AddCommands(commandTypes)
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