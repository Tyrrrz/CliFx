using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class ParameterBindingSpecs : SpecsBase
{
    public ParameterBindingSpecs(ITestOutputHelper testOutput)
        : base(testOutput)
    {
    }

    [Fact]
    public async Task Parameter_is_bound_from_an_argument_matching_its_order()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
[Command]
public class Command : ICommand
{
    [CommandParameter(0)]
    public string Foo { get; set; }
    
    [CommandParameter(1)]
    public string Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""Foo = "" + Foo);
        console.Output.WriteLine(""Bar = "" + Bar);
        
        return default;
    }
}");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            new[] {"one", "two"},
            new Dictionary<string, string>()
        );

        var stdOut = FakeConsole.ReadOutputString();

        // Assert
        exitCode.Should().Be(0);
        stdOut.Should().ConsistOfLines(
            "Foo = one",
            "Bar = two"
        );
    }

    [Fact]
    public async Task Parameter_of_non_scalar_type_is_bound_from_remaining_non_option_arguments()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
[Command]
public class Command : ICommand
{
    [CommandParameter(0)]
    public string Foo { get; set; }
    
    [CommandParameter(1)]
    public string Bar { get; set; }
    
    [CommandParameter(2)]
    public IReadOnlyList<string> Baz { get; set; }
    
    [CommandOption(""boo"")]
    public string Boo { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""Foo = "" + Foo);
        console.Output.WriteLine(""Bar = "" + Bar);
        
        foreach (var i in Baz)
            console.Output.WriteLine(""Baz = "" + i);
        
        return default;
    }
}");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            new[] {"one", "two", "three", "four", "five", "--boo", "xxx"},
            new Dictionary<string, string>()
        );

        var stdOut = FakeConsole.ReadOutputString();

        // Assert
        exitCode.Should().Be(0);
        stdOut.Should().ConsistOfLines(
            "Foo = one",
            "Bar = two",
            "Baz = three",
            "Baz = four",
            "Baz = five"
        );
    }

    [Fact]
    public async Task Parameter_binding_fails_if_one_of_the_parameters_has_not_been_provided()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
[Command]
public class Command : ICommand
{
    [CommandParameter(0)]
    public string Foo { get; set; }
    
    [CommandParameter(1)]
    public string Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            new[] {"one"},
            new Dictionary<string, string>()
        );

        var stdErr = FakeConsole.ReadErrorString();

        // Assert
        exitCode.Should().NotBe(0);
        stdErr.Should().Contain("Missing parameter(s)");
    }

    [Fact]
    public async Task Parameter_binding_fails_if_a_parameter_of_non_scalar_type_has_not_been_provided_with_at_least_one_value()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
[Command]
public class Command : ICommand
{
    [CommandParameter(0)]
    public string Foo { get; set; }
    
    [CommandParameter(1)]
    public IReadOnlyList<string> Bar { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            new[] {"one"},
            new Dictionary<string, string>()
        );

        var stdErr = FakeConsole.ReadErrorString();

        // Assert
        exitCode.Should().NotBe(0);
        stdErr.Should().Contain("Missing parameter(s)");
    }

    [Fact]
    public async Task Parameter_binding_fails_if_one_of_the_provided_parameters_is_unexpected()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            @"
[Command]
public class Command : ICommand
{
    [CommandParameter(0)]
    public string Foo { get; set; }
    
    [CommandParameter(1)]
    public string Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}");

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            new[] {"one", "two", "three"},
            new Dictionary<string, string>()
        );

        var stdErr = FakeConsole.ReadErrorString();

        // Assert
        exitCode.Should().NotBe(0);
        stdErr.Should().Contain("Unexpected parameter(s)");
    }
}