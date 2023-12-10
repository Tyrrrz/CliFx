using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class ApplicationSpecs(ITestOutputHelper testOutput) : SpecsBase(testOutput)
{
    [Fact]
    public async Task I_can_create_an_application_with_the_default_configuration()
    {
        // Act
        var app = new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .UseConsole(FakeConsole)
            .Build();

        var exitCode = await app.RunAsync(Array.Empty<string>(), new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task I_can_create_an_application_with_a_custom_configuration()
    {
        // Act
        var app = new CliApplicationBuilder()
            .AddCommand<NoOpCommand>()
            .AddCommandsFrom(typeof(NoOpCommand).Assembly)
            .AddCommands(new[] { typeof(NoOpCommand) })
            .AddCommandsFrom(new[] { typeof(NoOpCommand).Assembly })
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

        var exitCode = await app.RunAsync(Array.Empty<string>(), new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task I_can_try_to_create_an_application_and_get_an_error_if_it_has_invalid_commands()
    {
        // Act
        var app = new CliApplicationBuilder()
            .AddCommand(typeof(ApplicationSpecs))
            .UseConsole(FakeConsole)
            .Build();

        var exitCode = await app.RunAsync(Array.Empty<string>(), new Dictionary<string, string>());

        // Assert
        exitCode.Should().NotBe(0);

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().Contain("not a valid command");
    }
}
