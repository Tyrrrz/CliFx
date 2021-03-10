using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class ExecutionSpecs : SpecsBase
    {
        public ExecutionSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        [Fact]
        public async Task Application_exits_with_a_zero_exit_code_if_the_command_completes_successfully()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<NoOpCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            // Assert
            exitCode.Should().Be(0);
        }
    }
}