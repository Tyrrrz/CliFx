using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CliFx.Directives;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public partial class DirectivesSpecs
    {
        [Fact]
        public async Task Preview_directive_can_be_specified_to_print_provided_arguments_as_they_were_parsed()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(NamedCommand))
                .UseConsole(console)
                .AddDirective<PreviewDirective>()
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] { "[preview]", "cmd", "param", "-abc", "--option", "foo" },
                new Dictionary<string, string>());

            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().Be(0);
            stdOutData.Should().ContainAll("cmd", "<param>", "[-a]", "[-b]", "[-c]", "[--option \"foo\"]");
        }
    }
}