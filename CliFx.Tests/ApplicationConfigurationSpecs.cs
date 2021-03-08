using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using CliFx.Tests.Commands;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class ApplicationConfigurationSpecs : IDisposable
    {
        private readonly ITestOutputHelper _testOutput;
        private readonly FakeInMemoryConsole _console = new();

        public ApplicationConfigurationSpecs(ITestOutputHelper testOutput) =>
            _testOutput = testOutput;

        public void Dispose()
        {
            _console.DumpToTestOutput(_testOutput);
            _console.Dispose();
        }

        [Fact]
        public async Task Application_can_be_created_with_the_default_configuration()
        {
            // Act
            var app = new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseConsole(_console)
                .Build();

            var exitCode = await app.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            // Assert
            exitCode.Should().Be(0);
        }

        [Fact]
        public async Task Application_can_be_created_with_a_customized_configuration()
        {
            // Act
            var app = new CliApplicationBuilder()
                .AddCommand<DefaultCommand>()
                .AddCommandsFrom(typeof(DefaultCommand).Assembly)
                .AddCommands(new[] {typeof(DefaultCommand)})
                .AddCommandsFrom(new[] {typeof(DefaultCommand).Assembly})
                .AddCommandsFromThisAssembly()
                .AllowDebugMode()
                .AllowPreviewMode()
                .UseTitle("test")
                .UseExecutableName("test")
                .UseVersionText("test")
                .UseDescription("test")
                .UseConsole(_console)
                .UseTypeActivator(Activator.CreateInstance!)
                .Build();

            var exitCode = await app.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            // Assert
            exitCode.Should().Be(0);
        }
    }
}