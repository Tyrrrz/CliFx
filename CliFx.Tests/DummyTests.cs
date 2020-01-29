using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CliWrap;
using FluentAssertions;
using NUnit.Framework;

namespace CliFx.Tests
{
    [TestFixture]
    public class DummyTests
    {
        private static Assembly DummyAssembly { get; } = typeof(Dummy.Program).Assembly;

        private static IEnumerable<TestCaseData> GetTestCases_RunAsync()
        {
            yield return new TestCaseData(
                new[] {"--version"},
                new Dictionary<string, string>(),
                $"v{DummyAssembly.GetName().Version}"
            );

            yield return new TestCaseData(
                new string[0],
                new Dictionary<string, string>(),
                "Hello World!"
            );

            yield return new TestCaseData(
                new[] {"--target", "Earth"},
                new Dictionary<string, string>(),
                "Hello Earth!"
            );

            yield return new TestCaseData(
                new string[0],
                new Dictionary<string, string>
                {
                    ["ENV_TARGET"] = "Mars"
                },
                "Hello Mars!"
            );

            yield return new TestCaseData(
                new[] {"--target", "Earth"},
                new Dictionary<string, string>
                {
                    ["ENV_TARGET"] = "Mars"
                },
                "Hello Earth!"
            );
        }

        [TestCaseSource(nameof(GetTestCases_RunAsync))]
        public async Task RunAsync_Test(
            IReadOnlyList<string> arguments,
            IReadOnlyDictionary<string, string> environmentVariables,
            string expectedStdOut)
        {
            // Arrange
            var cli = Cli.Wrap("dotnet")
                .SetArguments(arguments.Prepend(DummyAssembly.Location).ToArray())
                .EnableExitCodeValidation()
                .EnableStandardErrorValidation()
                .SetStandardOutputCallback(Console.WriteLine)
                .SetStandardErrorCallback(Console.WriteLine);

            foreach (var (key, value) in environmentVariables)
                cli.SetEnvironmentVariable(key, value);

            // Act
            var result = await cli.ExecuteAsync();

            // Assert
            result.ExitCode.Should().Be(0);
            result.StandardError.Should().BeNullOrWhiteSpace();
            result.StandardOutput.TrimEnd().Should().Be(expectedStdOut);
        }
    }
}