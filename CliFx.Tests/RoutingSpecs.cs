using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public partial class RoutingSpecs
    {
        [Fact]
        public async Task Default_command_is_executed_if_provided_arguments_do_not_match_any_named_command()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(DefaultCommand))
                .AddCommand(typeof(ConcatCommand))
                .AddCommand(typeof(DivideCommand))
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>(), new Dictionary<string, string>());
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().Be("Hello world!");
        }

        [Fact]
        public async Task Help_text_is_printed_if_no_arguments_were_provided_and_default_command_is_not_defined()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(ConcatCommand))
                .AddCommand(typeof(DivideCommand))
                .UseConsole(console)
                .UseDescription("This will be visible in help")
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>(), new Dictionary<string, string>());
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().Contain("This will be visible in help");
        }

        [Fact]
        public async Task Specific_named_command_is_executed_if_provided_arguments_match_its_name()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(DefaultCommand))
                .AddCommand(typeof(ConcatCommand))
                .AddCommand(typeof(DivideCommand))
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"concat", "-i", "foo", "bar", "-s", ", "}, new Dictionary<string, string>());
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().Be("foo, bar");
        }
    }
}