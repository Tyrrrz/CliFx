using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Commands;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class ApplicationConfigurationSpecs : SpecsBase
    {
        public ApplicationConfigurationSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        [Fact]
        public async Task Application_can_be_created_with_the_default_configuration()
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
    }
}