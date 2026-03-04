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
            .AddCommand(NoOpCommand.Schema)
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
            .AddCommand(NoOpCommand.Schema)
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
}
