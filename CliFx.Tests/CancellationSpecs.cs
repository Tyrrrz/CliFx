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
        public async Task If_the_execution_is_cancelled_then_the_application_terminates_immediately_with_a_non_zero_exit_code()
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
            stdOut.Should().BeNullOrWhiteSpace();
        }
    }
}