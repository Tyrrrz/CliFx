using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public partial class DirectivesSpecs
    {
        [Fact]
        public async Task Preview_directive_can_be_enabled_to_print_provided_arguments_as_they_were_parsed()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(NamedCommand))
                .UseConsole(console)
                .AllowPreviewMode()
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"[preview]", "cmd", "param", "-abc", "--option", "foo"}, new Dictionary<string, string>());
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAll("cmd", "<param>", "[-a]", "[-b]", "[-c]", "[--option foo]");
        }
    }
}