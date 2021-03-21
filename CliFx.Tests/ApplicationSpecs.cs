using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class ApplicationSpecs : SpecsBase
    {
        public ApplicationSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        [Fact]
        public async Task Application_can_be_created_with_minimal_configuration()
        {
            // Act
            var app = new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseConsole(FakeConsole)
                .Build();

            var exitCode = await app.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            // Assert
            exitCode.Should().Be(0);
        }

        [Fact]
        public async Task Application_can_be_created_with_a_fully_customized_configuration()
        {
            // Act
            var app = new CliApplicationBuilder()
                .AddCommand<NoOpCommand>()
                .AddCommandsFrom(typeof(NoOpCommand).Assembly)
                .AddCommands(new[] {typeof(NoOpCommand)})
                .AddCommandsFrom(new[] {typeof(NoOpCommand).Assembly})
                .AddCommandsFromThisAssembly()
                .AllowDebugMode()
                .AllowPreviewMode()
                .SetTitle("test")
                .SetExecutableName("test")
                .SetVersion("test")
                .SetDescription("test")
                .UseConsole(FakeConsole)
                .UseTypeActivator(Activator.CreateInstance!)
                .Build();

            var exitCode = await app.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            // Assert
            exitCode.Should().Be(0);
        }

        [Fact]
        public async Task Application_configuration_fails_if_an_invalid_command_is_registered()
        {
            // Act
            var app = new CliApplicationBuilder()
                .AddCommand(typeof(ApplicationSpecs))
                .UseConsole(FakeConsole)
                .Build();

            var exitCode = await app.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().Contain("not a valid command");
        }
    }
}