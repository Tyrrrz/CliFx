using System;
using System.Threading;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using CliFx.Tests.Commands;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public class CancellationSpecs
    {
        [Fact]
        public async Task Command_can_perform_additional_cleanup_if_cancellation_is_requested()
        {
            // Can't test it with a real console because CliWrap can't send Ctrl+C

            // Arrange
            using var cts = new CancellationTokenSource();
            var (console, stdOut, _) = VirtualConsole.CreateBuffered(cts.Token);

            var application = new CliApplicationBuilder()
                .AddCommand<CancellableCommand>()
                .UseConsole(console)
                .Build();

            // Act
            cts.CancelAfter(TimeSpan.FromSeconds(0.2));

            var exitCode = await application.RunAsync(new[] {"cmd"});

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.GetString().Trim().Should().Be(CancellableCommand.CancellationOutputText);
        }
    }
}