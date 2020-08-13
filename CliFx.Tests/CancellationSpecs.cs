namespace CliFx.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public partial class CancellationSpecs
    {
        [Fact]
        public async Task Command_can_perform_additional_cleanup_if_cancellation_is_requested()
        {
            // Can't test it with a real console because CliWrap can't send Ctrl+C

            // Arrange
            using var cts = new CancellationTokenSource();

            await using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut, cancellationToken: cts.Token);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(CancellableCommand))
                .UseConsole(console)
                .Build();

            // Act
            cts.CancelAfter(TimeSpan.FromSeconds(0.2));

            var exitCode = await application.RunAsync(
                new[] { "cancel" },
                new Dictionary<string, string>());

            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdOutData.Should().Be("Cancellation requested");
        }
    }
}