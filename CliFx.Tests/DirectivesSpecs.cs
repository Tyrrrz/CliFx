using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CliFx.Tests.Commands;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class DirectivesSpecs
    {
        private readonly ITestOutputHelper _output;

        public DirectivesSpecs(ITestOutputHelper output) => _output = output;

        [Fact]
        public async Task Preview_directive_can_be_specified_to_print_provided_arguments_as_they_were_parsed()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<NamedCommand>()
                .UseConsole(console)
                .AllowPreviewMode()
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"[preview]", "named", "param", "-abc", "--option", "foo"},
                new Dictionary<string, string>());

            // Assert
            exitCode.Should().Be(0);
            stdOut.GetString().Should().ContainAll(
                "named", "<param>", "[-a]", "[-b]", "[-c]", "[--option \"foo\"]"
            );

            _output.WriteLine(stdOut.GetString());
        }
    }
}