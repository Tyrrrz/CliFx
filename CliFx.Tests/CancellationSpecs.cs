using System;
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
            // TODO: test at higher level?
            // Can't test it with a real console because CliWrap can't send Ctrl+C (yet)

            // Arrange
            using var console = new FakeInMemoryConsole();

            var application = new CliApplicationBuilder()
                .AddCommand<CancellableCommand>()
                .UseConsole(console)
                .Build();

            // Act
            console.RequestCancellation(TimeSpan.FromSeconds(0.2));

            var exitCode = await application.RunAsync(new[] {"cmd"});
            var stdOut = console.ReadOutputString();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Trim().Should().Be(CancellableCommand.CancellationOutputText);
        }
    }
}