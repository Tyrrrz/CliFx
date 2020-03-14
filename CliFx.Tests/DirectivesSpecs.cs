using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public partial class DirectivesSpecs
    {
        [Fact]
        public async Task If_the_arguments_contain_the_preview_directive_then_the_parsed_input_is_printed()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(NamedCommand))
                .UseConsole(console)
                .AllowPreviewMode()
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"[preview]", "concat", "-s", "_", "-i", "foo", "bar"}, new Dictionary<string, string>());
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAll("concat", "-s", "_", "-i", "foo", "bar");
        }
    }
}