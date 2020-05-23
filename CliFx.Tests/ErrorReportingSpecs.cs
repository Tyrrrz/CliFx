﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public partial class ErrorReportingSpecs
    {
        private readonly ITestOutputHelper _output;

        public ErrorReportingSpecs(ITestOutputHelper output) => _output = output;

        [Fact]
        public async Task Command_may_throw_a_generic_exception_which_exits_and_prints_error_message_and_stack_trace()
        {
            // Arrange
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(GenericExceptionCommand))
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"exc", "-m", "Kaput"},
                new Dictionary<string, string>());

            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().ContainAll(
                "System.Exception:",
                "Kaput", "at",
                "CliFx.Tests");

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_may_throw_a_specialized_exception_which_exits_with_custom_code_and_prints_minimal_error_details()
        {
            // Arrange
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(CommandExceptionCommand))
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"exc", "-m", "Kaput", "-c", "69"},
                new Dictionary<string, string>());

            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().Be(69);
            stdErrData.Should().Be("Kaput");

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_may_throw_a_specialized_exception_without_error_message_which_exits_and_prints_full_error_details()
        {
            // Arrange
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(CommandExceptionCommand))
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"exc"},
                new Dictionary<string, string>());

            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().ContainAll(
                "CliFx.Exceptions.CommandException:",
                "at",
                "CliFx.Tests");

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_may_throw_a_specialized_exception_which_exits_and_prints_help_text()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            await using var stdErr = new MemoryStream();

            var console = new VirtualConsole(output: stdOut, error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(CommandExceptionCommand))
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"exc", "-m", "Kaput", "--show-help"},
                new Dictionary<string, string>());

            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();
            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().Be("Kaput");

            stdOutData.Should().ContainAll(
                "Usage",
                "Options",
                "-h|--help", "Shows help text."
            );

            _output.WriteLine(stdErrData);
            _output.WriteLine(stdOutData);
        }

        [Fact]
        public async Task Command_shows_help_text_on_invalid_user_input()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(output: stdOut, error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(CommandExceptionCommand))
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"not-a-valid-command", "-r", "foo"},
                new Dictionary<string, string>());

            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();
            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeNullOrWhiteSpace();

            stdOutData.Should().ContainAll(
                "Usage",
                "[command]",
                "Options",
                "-h|--help", "Shows help text."
            );

            _output.WriteLine(stdErrData);
            _output.WriteLine(stdOutData);
        }
    }
}