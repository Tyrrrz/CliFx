using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Models;
using NUnit.Framework;

namespace CliFx.Tests
{
    public partial class CliApplicationTests
    {
        [Command]
        private class TestDefaultCommand : ICommand
        {
            public Task ExecuteAsync(CommandContext context) => Task.CompletedTask;
        }

        [Command("command")]
        private class TestNamedCommand : ICommand
        {
            public Task ExecuteAsync(CommandContext context) => Task.CompletedTask;
        }

        [Command("faulty-command")]
        private class FaultyCommand : ICommand
        {
            public Task ExecuteAsync(CommandContext context) => Task.FromException(new CommandErrorException(-1337));
        }
    }

    [TestFixture]
    public partial class CliApplicationTests
    {
        private static IEnumerable<TestCaseData> GetTestCases_RunAsync()
        {
            // Specified command is defined

            yield return new TestCaseData(
                new[] {typeof(TestNamedCommand)},
                new[] {"command"}
            );

            yield return new TestCaseData(
                new[] {typeof(TestNamedCommand)},
                new[] {"command", "--help"}
            );

            yield return new TestCaseData(
                new[] {typeof(TestNamedCommand)},
                new[] {"command", "-h"}
            );

            yield return new TestCaseData(
                new[] {typeof(TestNamedCommand)},
                new[] {"command", "-?"}
            );


            // Default command is defined

            yield return new TestCaseData(
                new[] {typeof(TestDefaultCommand)},
                new string[0]
            );

            yield return new TestCaseData(
                new[] {typeof(TestDefaultCommand)},
                new[] {"--version"}
            );

            yield return new TestCaseData(
                new[] {typeof(TestDefaultCommand)},
                new[] {"--help"}
            );

            yield return new TestCaseData(
                new[] {typeof(TestDefaultCommand)},
                new[] {"-h"}
            );

            yield return new TestCaseData(
                new[] {typeof(TestDefaultCommand)},
                new[] {"-?"}
            );

            // Default command is not defined

            yield return new TestCaseData(
                new Type[0],
                new string[0]
            );

            yield return new TestCaseData(
                new Type[0],
                new[] {"--version"}
            );

            yield return new TestCaseData(
                new Type[0],
                new[] {"--help"}
            );

            yield return new TestCaseData(
                new Type[0],
                new[] {"-h"}
            );

            yield return new TestCaseData(
                new Type[0],
                new[] {"-?"}
            );

            // Specified a faulty command

            yield return new TestCaseData(
                new[] {typeof(FaultyCommand)},
                new[] {"--version"}
            );

            yield return new TestCaseData(
                new[] {typeof(FaultyCommand)},
                new[] {"--help"}
            );

            yield return new TestCaseData(
                new[] {typeof(FaultyCommand)},
                new[] {"-h"}
            );

            yield return new TestCaseData(
                new[] {typeof(FaultyCommand)},
                new[] {"-?"}
            );
        }

        private static IEnumerable<TestCaseData> GetTestCases_RunAsync_Negative()
        {
            // Specified command is not defined

            yield return new TestCaseData(
                new Type[0],
                new[] {"command"}
            );

            yield return new TestCaseData(
                new Type[0],
                new[] {"command", "--help"}
            );

            yield return new TestCaseData(
                new Type[0],
                new[] {"command", "-h"}
            );

            yield return new TestCaseData(
                new Type[0],
                new[] {"command", "-?"}
            );

            // Specified a faulty command

            yield return new TestCaseData(
                new[] {typeof(FaultyCommand)},
                new[] {"faulty-command"}
            );
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_RunAsync))]
        public async Task RunAsync_Test(IReadOnlyList<Type> commandTypes, IReadOnlyList<string> commandLineArguments)
        {
            // Arrange
            var application = new CliApplication(commandTypes);

            // Act
            var exitCodeValue = await application.RunAsync(commandLineArguments);

            // Assert
            Assert.That(exitCodeValue, Is.Zero, "Exit code");
        }

        [Test]
        [TestCaseSource(nameof(GetTestCases_RunAsync_Negative))]
        public async Task RunAsync_Negative_Test(IReadOnlyList<Type> commandTypes, IReadOnlyList<string> commandLineArguments)
        {
            // Arrange
            var application = new CliApplication(commandTypes);

            // Act
            var exitCodeValue = await application.RunAsync(commandLineArguments);

            // Assert
            Assert.That(exitCodeValue, Is.Not.Zero, "Exit code");
        }
    }
}