using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using CliFx.Tests.Commands;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class DirectivesSpecs : IDisposable
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly FakeInMemoryConsole _console = new();

        public DirectivesSpecs(ITestOutputHelper testOutput) =>
            _testOutput = testOutput;

        public void Dispose()
        {
            _console.DumpToTestOutput(_testOutput);
            _console.Dispose();
        }

        [Fact]
        public async Task Preview_directive_can_be_specified_to_print_provided_arguments_as_they_were_parsed()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<NamedCommand>()
                .UseConsole(_console)
                .AllowPreviewMode()
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"[preview]", "named", "param", "-abc", "--option", "foo"},
                new Dictionary<string, string>()
            );

            var stdOut = _console.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAll(
                "named", "<param>", "[-a]", "[-b]", "[-c]", "[--option \"foo\"]"
            );
        }
    }
}