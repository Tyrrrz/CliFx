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

        [Theory]
        [InlineData(new[] { "[suggest]" }, new[] { "named" })]
        [InlineData(new[] { "[suggest]", "n" }, new[] { "named" })]
        [InlineData(new[] { "[suggest]", "named" }, new string[] { })]
        [InlineData(new[] { "[suggest]", "named_badly" }, new string[] { })]
        public async Task Suggestion_directive_returns_command_suggestions(string[] input, string[] expected)
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
                input,
                new Dictionary<string, string>()
            );

            // Assert
            exitCode.Should().Be(0);
            var outputSet = stdOut.GetString().Split(Environment.NewLine).Where(p=>!string.IsNullOrWhiteSpace(p)).ToHashSet();
            var expectedSet = expected.ToHashSet();

            outputSet.Should().BeEquivalentTo(expectedSet);

            _output.WriteLine(stdOut.GetString());
        }


        [Theory]
        [InlineData(new[] { "[suggest]" }, new[] { "named", "named sub" })]
        [InlineData(new[] { "[suggest]", "n" }, new[] { "named", "named sub" })]
        [InlineData(new[] { "[suggest]", "named" }, new string[] {})]
        [InlineData(new[] { "[suggest]", "named_badly" }, new string[] {})]
        [InlineData(new[] { "[suggest]", "named s" }, new string[] { "named sub" })]
        [InlineData(new[] { "[suggest]", "named sub" }, new string[] {})]
        public async Task Suggestion_directive_returns_subcommand_suggestions(string[] input, string[] expected)
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<NamedCommand>()
                .AddCommand<NamedSubCommand>()
                .UseConsole(console)
                .AllowSuggestMode()
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                input,
                new Dictionary<string, string>()
            );

            // Assert
            exitCode.Should().Be(0);
            var outputSet = stdOut.GetString().Split(Environment.NewLine).Where(p =>!string.IsNullOrWhiteSpace(p)).ToHashSet();
            var expectedSet = expected.ToHashSet();

            outputSet.Should().BeEquivalentTo(expectedSet);

            _output.WriteLine(stdOut.GetString());
        }
    }
}
