using System;
using System.IO;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using CliFx.Tests.Commands;
using CliFx.Tests.Commands.Invalid;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class ApplicationSpecs
    {
        private readonly ITestOutputHelper _output;

        public ApplicationSpecs(ITestOutputHelper output) => _output = output;

        [Fact]
        public void Application_can_be_created_with_a_default_configuration()
        {
            // Act
            var app = new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .Build();

            // Assert
            app.Should().NotBeNull();
        }

        [Fact]
        public void Application_can_be_created_with_a_custom_configuration()
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
                .UseConsole(new RedirectedConsole(Stream.Null))
                .UseTypeActivator(Activator.CreateInstance!)
                .Build();

            // Assert
            app.Should().NotBeNull();
        }

        [Fact]
        public async Task At_least_one_command_must_be_defined_in_an_application()
        {
            var (console, _, stdErr) = RedirectedConsole.CreateBuffered();

            var application = new CliApplicationBuilder()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.GetString().Should().NotBeNullOrWhiteSpace();

            _output.WriteLine(stdErr.GetString());
        }
    }
}