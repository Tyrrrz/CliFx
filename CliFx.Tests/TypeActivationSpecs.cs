using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class TypeActivationSpecs : SpecsBase
{
    public TypeActivationSpecs(ITestOutputHelper testOutput)
        : base(testOutput)
    {
    }

    [Fact]
    public async Task Default_type_activator_can_initialize_a_type_if_it_has_a_parameterless_constructor()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
[Command]
public class Command : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""foo"");
        return default;
    }
}");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .UseTypeActivator(new DefaultTypeActivator())
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

        var stdOut = FakeConsole.ReadOutputString();

        // Assert
        exitCode.Should().Be(0);
        stdOut.Trim().Should().Be("foo");
    }

    [Fact]
    public async Task Default_type_activator_fails_if_the_type_does_not_have_a_parameterless_constructor()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
[Command]
public class Command : ICommand
{
    public Command(string foo) {}

    public ValueTask ExecuteAsync(IConsole console) => default;
}");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .UseTypeActivator(new DefaultTypeActivator())
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

        var stdErr = FakeConsole.ReadErrorString();

        // Assert
        exitCode.Should().NotBe(0);
        stdErr.Should().Contain("Failed to create an instance of type");
    }

    [Fact]
    public async Task Delegate_type_activator_can_initialize_a_type_using_a_custom_function()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
[Command]
public class Command : ICommand
{
    private readonly string _foo;

    public Command(string foo) => _foo = foo;

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(_foo);
        return default;
    }
}");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .UseTypeActivator(type => Activator.CreateInstance(type, "hello world")!)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

        var stdOut = FakeConsole.ReadOutputString();

        // Assert
        exitCode.Should().Be(0);
        stdOut.Trim().Should().Be("hello world");
    }

    [Fact]
    public async Task Delegate_type_activator_fails_if_the_underlying_function_returns_null()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
[Command]
public class Command : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""foo"");
        return default;
    }
}");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .UseTypeActivator(_ => null!)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

        var stdErr = FakeConsole.ReadErrorString();

        // Assert
        exitCode.Should().NotBe(0);
        stdErr.Should().Contain("Failed to create an instance of type");
    }
}