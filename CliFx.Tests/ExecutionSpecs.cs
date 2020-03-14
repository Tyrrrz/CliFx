using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.TestCommands;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public class ExecutionSpecs
    {
        [Fact]
        public async Task If_no_arguments_are_provided_and_the_default_command_is_defined_then_that_command_is_executed()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(HelloWorldDefaultCommand))
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
        public async Task If_no_arguments_are_provided_and_the_default_command_is_not_defined_then_the_application_help_is_printed()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(ConcatCommand))
                .AddCommand(typeof(DivideCommand))
                .UseConsole(console)
                .UseDescription("App description is shown in root help text")
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>(), new Dictionary<string, string>());
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().Contain("App description is shown in root help text");
        }

        [Fact]
        public async Task If_arguments_contain_the_help_option_but_do_not_match_a_named_command_then_the_application_help_is_printed()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(HelloWorldDefaultCommand))
                .AddCommand(typeof(ConcatCommand))
                .AddCommand(typeof(DivideCommand))
                .UseConsole(console)
                .UseDescription("App description is shown in root help text")
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"-h"}, new Dictionary<string, string>());
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().Contain("App description is shown in root help text");
        }

        [Fact]
        public async Task If_the_arguments_match_a_specific_named_command_then_that_command_is_executed()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(HelloWorldDefaultCommand))
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

        [Fact]
        public async Task If_the_arguments_match_a_specific_named_command_and_contain_the_help_option_then_the_help_for_that_command_is_printed()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(HelloWorldDefaultCommand))
                .AddCommand(typeof(ConcatCommand))
                .AddCommand(typeof(DivideCommand))
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[] {"concat", "-h"}, new Dictionary<string, string>());
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().Contain("String separator.");
        }
    }
}