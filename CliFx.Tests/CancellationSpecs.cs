using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Commands;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class CancellationSpecs : SpecsBase
    {
        public CancellationSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        [Fact]
        public async Task Command_can_perform_additional_cleanup_if_cancellation_is_requested()
        {
            // Can't test it with a real console because CliWrap can't send Ctrl+C (yet)

            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<CancellableCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            FakeConsole.RequestCancellation(TimeSpan.FromSeconds(0.2));

            var exitCode = await application.RunAsync(
                new[] {"cmd"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Trim().Should().Be(CancellableCommand.CancellationOutputText);
        }
    }
}