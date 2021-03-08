using System;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using CliFx.Tests.Commands;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class RoutingSpecs : IDisposable
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly FakeInMemoryConsole _console = new();

        public RoutingSpecs(ITestOutputHelper testOutput) =>
            _testOutput = testOutput;

        public void Dispose()
        {
            _console.DumpToTestOutput(_testOutput);
            _console.Dispose();
        }

        [Fact]
        public async Task Default_command_is_executed_if_provided_arguments_do_not_match_any_named_command()
        {
            // Arrange

            var application = new CliApplicationBuilder()
                .AddCommand<DefaultCommand>()
                .AddCommand<NamedCommand>()
                .AddCommand<NamedSubCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());

            var stdOut = _console.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be(DefaultCommand.ExpectedOutputText);
        }

        [Fact]
        public async Task Specific_named_command_is_executed_if_provided_arguments_match_its_name()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<DefaultCommand>()
                .AddCommand<NamedCommand>()
                .AddCommand<NamedSubCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"named"});

            var stdOut = _console.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be(NamedCommand.ExpectedOutputText);
        }

        [Fact]
        public async Task Specific_named_sub_command_is_executed_if_provided_arguments_match_its_name()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<DefaultCommand>()
                .AddCommand<NamedCommand>()
                .AddCommand<NamedSubCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"named", "sub"});

            var stdOut = _console.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be(NamedSubCommand.ExpectedOutputText);
        }

        [Fact]
        public async Task Help_text_is_printed_if_no_arguments_were_provided_and_default_command_is_not_defined()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<NamedCommand>()
                .AddCommand<NamedSubCommand>()
                .UseConsole(_console)
                .UseDescription("This will be visible in help")
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());

            var stdOut = _console.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().Contain("This will be visible in help");
        }

        [Fact]
        public async Task Help_text_is_printed_if_provided_arguments_contain_the_help_option()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<DefaultCommand>()
                .AddCommand<NamedCommand>()
                .AddCommand<NamedSubCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"--help"});

            var stdOut = _console.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAll(
                "Default command description",
                "Usage"
            );
        }

        [Fact]
        public async Task Help_text_is_printed_if_provided_arguments_contain_the_help_option_even_if_default_command_is_not_defined()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<NamedCommand>()
                .AddCommand<NamedSubCommand>()
                .UseDescription("This will be visible in help")
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"--help"});

            var stdOut = _console.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().Contain("This will be visible in help");
        }

        [Fact]
        public async Task Help_text_for_a_specific_named_command_is_printed_if_provided_arguments_match_its_name_and_contain_the_help_option()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<DefaultCommand>()
                .AddCommand<NamedCommand>()
                .AddCommand<NamedSubCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"named", "--help"});

            var stdOut = _console.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAll(
                "Named command description",
                "Usage",
                "named"
            );
        }

        [Fact]
        public async Task Help_text_for_a_specific_named_sub_command_is_printed_if_provided_arguments_match_its_name_and_contain_the_help_option()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<DefaultCommand>()
                .AddCommand<NamedCommand>()
                .AddCommand<NamedSubCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"named", "sub", "--help"});

            var stdOut = _console.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAll(
                "Named sub command description",
                "Usage",
                "named", "sub"
            );
        }

        [Fact]
        public async Task Version_text_is_printed_if_the_only_provided_argument_is_the_version_option()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<DefaultCommand>()
                .AddCommand<NamedCommand>()
                .AddCommand<NamedSubCommand>()
                .UseVersionText("v6.9")
                .UseConsole(_console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"--version"});

            var stdOut = _console.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("v6.9");
        }
    }
}