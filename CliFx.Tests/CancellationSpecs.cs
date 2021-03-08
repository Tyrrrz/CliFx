using System;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using CliFx.Tests.Commands;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class CancellationSpecs : IDisposable
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly FakeInMemoryConsole _console = new();

        public CancellationSpecs(ITestOutputHelper testOutput) =>
            _testOutput = testOutput;

        public void Dispose()
        {
            _console.DumpToTestOutput(_testOutput);
            _console.Dispose();
        }

        [Fact]
        public async Task Command_can_perform_additional_cleanup_if_cancellation_is_requested()
        {
            // TODO: test at higher level?
            // Can't test it with a real console because CliWrap can't send Ctrl+C (yet)

            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<CancellableCommand>()
                .UseConsole(_console)
                .Build();

            // Act
            _console.RequestCancellation(TimeSpan.FromSeconds(0.2));

            var exitCode = await application.RunAsync(new[] {"cmd"});
            var stdOut = _console.ReadOutputString();

            // Assert
            exitCode.Should().NotBe(0);
            stdOut.Trim().Should().Be(CancellableCommand.CancellationOutputText);
        }
    }
}