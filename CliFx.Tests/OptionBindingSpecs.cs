using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class OptionBindingSpecs : SpecsBase
    {
        public OptionBindingSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        [Fact]
        public async Task Option_can_be_bound_from_multiple_values_even_if_the_arguments_use_mixed_naming()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"", 'f')]
    public IReadOnlyList<string>? Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console)
    {
        foreach (var i in Foo)
            console.Output.WriteLine(i);
            
        return default;
    }
}");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"--foo", "one", "-f", "two", "--foo", "three"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ConsistOfLines(
                "one",
                "two",
                "three"
            );
        }

        [Fact]
        public async Task Argument_that_begins_with_a_dash_followed_by_a_non_letter_character_is_parsed_as_a_value()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"")]
    public int? Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(Foo);
            
        return default;
    }
}");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"--foo", "-13"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("-13");
        }

        [Fact]
        public async Task Binding_fails_if_a_required_option_has_not_been_provided()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"", IsRequired = true)]
    public string? Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                Array.Empty<string>(),
                new Dictionary<string, string>()
            );

            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().Contain("Missing values for one or more required options");
        }

        [Fact]
        public async Task Binding_fails_if_a_required_option_has_been_provided_with_an_empty_value()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"", IsRequired = true)]
    public string? Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"--foo"},
                new Dictionary<string, string>()
            );

            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().Contain("Missing values for one or more required options");
        }

        [Fact]
        public async Task Binding_fails_if_a_required_option_of_non_scalar_type_has_not_been_provided_with_at_least_one_value()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"", IsRequired = true)]
    public IReadOnlyList<string>? Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"--foo"},
                new Dictionary<string, string>()
            );

            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().Contain("Missing values for one or more required options");
        }

        [Fact]
        public async Task Binding_fails_if_one_of_the_provided_option_names_is_not_recognized()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"")]
    public string? Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"--foo", "one", "--bar", "two"},
                new Dictionary<string, string>()
            );

            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().Contain("Unrecognized options provided");
        }
    }
}