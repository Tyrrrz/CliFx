using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Commands;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class RoutingSpecs : SpecsBase
    {
        public RoutingSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        [Fact]
        public async Task Default_command_is_executed_if_provided_arguments_do_not_match_any_named_command()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<DefaultCommand>()
                .AddCommand<NamedCommand>()
                .AddCommand<NamedSubCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

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
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"named"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

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
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"named", "sub"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

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
                .UseConsole(FakeConsole)
                .SetDescription("This will be visible in help")
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

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
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"--help"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAllInOrder(
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
                .SetDescription("This will be visible in help")
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"--help"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

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
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"named", "--help"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAllInOrder(
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
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"named", "sub", "--help"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ContainAllInOrder(
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
                .SetVersion("v6.9")
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"--version"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("v6.9");
        }
    }
}