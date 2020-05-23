using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Tests.Internal;
using CliWrap;
using CliWrap.Buffered;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public partial class DirectivesSpecs
    {
        [Fact]
        public async Task Debug_directive_can_be_specified_to_have_the_application_wait_until_debugger_is_attached()
        {
            // We can't actually attach a debugger in tests, so instead just cancel execution after some time

            // Arrange
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            var stdOut = new StringBuilder();

            var command = Cli.Wrap("dotnet")
                .WithArguments(a => a
                    .Add(Dummy.Program.Location)
                    .Add("[debug]"))
                .WithEnvironmentVariables(e => e
                    .Set("ENV_TARGET", "Mars")) | stdOut;

            // Act
            await command.ExecuteAsync(cts.Token).Task.IgnoreCancellation();
            var stdOutData = stdOut.ToString();

            // Assert
            stdOutData.Should().Contain("Attach debugger to");
        }

        [Fact]
        public async Task Preview_directive_can_be_specified_to_print_provided_arguments_as_they_were_parsed()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(NamedCommand))
                .UseConsole(console)
                .AllowPreviewMode()
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"[preview]", "cmd", "param", "-abc", "--option", "foo"},
                new Dictionary<string, string>());

            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().Be(0);
            stdOutData.Should().ContainAll("cmd", "<param>", "[-a]", "[-b]", "[-c]", "[--option \"foo\"]");
        }
    }
}