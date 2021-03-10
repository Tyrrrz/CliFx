using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class ParameterBindingSpecs : SpecsBase
    {
        public ParameterBindingSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        [Fact]
        public async Task Parameter_is_bound_directly_from_argument_value_according_to_its_order()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandParameter(0)]
    public string? Foo { get; set; }
    
    [CommandParameter(1)]
    public int? Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(Foo);
        console.Output.WriteLine(Bar);
        
        return default;
    }
}");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"one", "2"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ConsistOfLines(
                "one",
                "2"
            );
        }

        [Fact]
        public async Task Binding_fails_if_one_of_the_parameters_has_not_been_provided()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandParameter(0)]
    public string? Foo { get; set; }
    
    [CommandParameter(1)]
    public int? Bar { get; set; }

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
            stdErr.Should().Contain("Missing value for parameter");
        }

        [Fact]
        public async Task Binding_fails_if_a_parameter_of_non_scalar_type_has_not_been_provided_with_at_least_one_value()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandParameter(0)]
    public string? Foo { get; set; }
    
    [CommandParameter(1)]
    public IReadOnlyList<string>? Bar { get; set; }
    
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
            stdErr.Should().Contain("Missing value for parameter");
        }

        [Fact]
        public async Task Binding_fails_if_one_of_the_provided_parameters_is_unexpected()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandParameter(0)]
    public string? Foo { get; set; }
    
    [CommandParameter(1)]
    public int? Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"one", "2", "uno"},
                new Dictionary<string, string>()
            );

            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().Contain("Unrecognized parameters provided");
        }
    }
}