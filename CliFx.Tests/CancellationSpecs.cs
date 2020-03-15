using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public partial class CancellationSpecs
    {
        [Fact]
        public async Task Command_can_perform_additional_cleanup_if_cancellation_is_requested()
        {
            // Arrange
            using var console = new VirtualConsole();

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(CancellableCommand))
                .UseConsole(console)
                .Build();

            // Act
            console.CancelAfter(TimeSpan.FromSeconds(0.2));

            var exitCode = await application.RunAsync(new[] {"cancel"}, new Dictionary<string, string>());
            var stdOut = console.ReadOutputString().TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Should().Be("Cancellation requested");
        }
    }
}