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

        var exitCode = await app.RunAsync([], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);
    }
}
