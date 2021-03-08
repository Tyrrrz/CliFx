using CliFx.Tests.Commands;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class SuggestionSpecs
    {

        private readonly ITestOutputHelper _output;

        public SuggestionSpecs(ITestOutputHelper output) => _output = output;

        [Fact]
        public async Task Suggestion_directive_is_recognized_and_returns_zero_suggestions_for_non_matching_commands()
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<NamedCommand>()
                .UseConsole(console)
                .AllowSuggestMode()
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] { "[suggest]", "unnamed" },
                new Dictionary<string, string>()
            );

            // Assert
            exitCode.Should().Be(0);
            stdOut.GetString().Should().BeEmpty();

            _output.WriteLine(stdOut.GetString());
        }
    }
}
