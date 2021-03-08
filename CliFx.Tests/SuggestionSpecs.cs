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

        [Theory]
        [InlineData(new[] { "[suggest]", "cmd", "paramter" }, new string[] {})]
        public async Task Suggestion_directive_ignores_parameters(string[] input, string[] expected)
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<WithParametersCommand>()
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
            var outputSet = stdOut.GetString().Split(Environment.NewLine).Where(p => !string.IsNullOrWhiteSpace(p)).ToHashSet();
            var expectedSet = expected.ToHashSet();

            outputSet.Should().BeEquivalentTo(expectedSet);

            _output.WriteLine(stdOut.GetString());
        }

        [Theory]
        // [InlineData(new[] { "[suggest]", "cmd", " " }, new string[] { "Value1", "Value2", "Value3"})] // spaces eaten by Environment.GetCommandLineArgs, can't implement.
        [InlineData(new[] { "[suggest]", "cmd", "v" }, new string[] { "Value", "Value1", "Value2", "Value3" })]
        [InlineData(new[] { "[suggest]", "cmd", "Value" }, new string[] { "Value", "Value1", "Value2", "Value3" })]
        [InlineData(new[] { "[suggest]", "cmd", "Value4" }, new string[] {  })]
        [InlineData(new[] { "[suggest]", "cmd", "Value", "c" }, new string[] { "Custom", "Custom1", "Custom2", "Custom3" })]
        [InlineData(new[] { "[suggest]", "cmd", "Value", "Custom4" }, new string[] { })]
        [InlineData(new[] { "[suggest]", "cmd", "Value", "Custom4", "a" }, new string[] { })]
        [InlineData(new[] { "[suggest]", "cmd", "Value", "Custom4", "a", "b" }, new string[] { })]
        public async Task Suggestion_directive_suggests_enum_parameters(string[] input, string[] expected)
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<WithEnumParametersCommand>()
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
            var outputSet = stdOut.GetString().Split(Environment.NewLine).Where(p => !string.IsNullOrWhiteSpace(p)).ToHashSet();
            var expectedSet = expected.ToHashSet();

            outputSet.Should().BeEquivalentTo(expectedSet);

            _output.WriteLine(stdOut.GetString());
        }

        [Theory]
        [InlineData(new[] { "[suggest]", "-" }, new string[] { "-h", "--help" })]
        [InlineData(new[] { "[suggest]", "--" }, new string[] { "--help" })]
        [InlineData(new[] { "[suggest]", "cmd", "-" }, new string[] { "-a", "-b", "-c", "-h", "--opt-aa", "--opt-bb", "--opt-cc", "--help" })]
        [InlineData(new[] { "[suggest]", "cmd", "--" }, new string[] { "--opt-aa", "--opt-bb", "--opt-cc", "--help" })]
        [InlineData(new[] { "[suggest]", "cmd", "--opt-a" }, new string[] { "--opt-aa" })]
        [InlineData(new[] { "[suggest]", "cmd", "--opt-aa" }, new string[] { "--opt-aa" })]
        [InlineData(new[] { "[suggest]", "cmd", "--opt-d" }, new string[] { })]
        public async Task Suggestion_directive_suggests_options(string[] input, string[] expected)
        {
            // Arrange
            var (console, stdOut, _) = VirtualConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .AddCommand<WithSuggestedOptionsCommand>()
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
            var outputSet = stdOut.GetString().Split(Environment.NewLine).Where(p => !string.IsNullOrWhiteSpace(p)).ToHashSet();
            var expectedSet = expected.ToHashSet();

            outputSet.Should().BeEquivalentTo(expectedSet);

            _output.WriteLine(stdOut.GetString());
        }
    }
}
