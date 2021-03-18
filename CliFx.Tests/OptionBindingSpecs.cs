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
        public async Task Option_is_bound_from_a_set_of_arguments_that_match_its_name()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"")]
    public string Foo { get; set; }
    
    [CommandOption(""bar"")]
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
                new[] {"--foo", "one", "--bar", "two"},
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
        public async Task Option_is_bound_from_a_set_of_arguments_that_match_its_short_name()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public string Foo { get; set; }
    
    [CommandOption('b')]
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
                new[] {"-f", "one", "-b", "two"},
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
        public async Task Option_is_bound_from_a_stack_of_arguments_if_one_of_them_matches_its_short_name()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public string Foo { get; set; }
    
    [CommandOption('b')]
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
                new[] {"-fb", "value"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ConsistOfLines(
                "Foo = ",
                "Bar = value"
            );
        }

        [Fact]
        public async Task Option_of_non_scalar_type_is_bound_from_a_set_of_arguments_that_match_its_name()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""Foo"")]
    public IReadOnlyList<string> Foo { get; set; }
    
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
                new[] {"--foo", "one", "two", "three"},
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
        public async Task Option_of_non_scalar_type_is_bound_from_a_set_of_arguments_that_match_its_short_name()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public IReadOnlyList<string> Foo { get; set; }
    
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
                new[] {"-f", "one", "two", "three"},
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
        public async Task Option_of_non_scalar_type_is_bound_from_multiple_sets_of_arguments_that_match_its_name()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"")]
    public IReadOnlyList<string> Foo { get; set; }
    
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
                new[] {"--foo", "one", "--foo", "two", "--foo", "three"},
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
        public async Task Option_of_non_scalar_type_is_bound_from_multiple_sets_of_arguments_that_match_its_short_name()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public IReadOnlyList<string> Foo { get; set; }
    
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
                new[] {"-f", "one", "-f", "two", "-f", "three"},
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
        public async Task Option_of_non_scalar_type_is_bound_from_multiple_sets_of_arguments_that_match_its_name_or_short_name()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"", 'f')]
    public IReadOnlyList<string> Foo { get; set; }
    
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
        public async Task Option_binding_does_not_consider_a_negative_number_as_an_option_name_or_short_name()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"")]
    public string Foo { get; set; }
    
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
        public async Task Option_binding_fails_if_a_required_option_has_not_been_provided()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"", IsRequired = true)]
    public string Foo { get; set; }
    
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
        public async Task Option_binding_fails_if_a_required_option_has_been_provided_with_an_empty_value()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"", IsRequired = true)]
    public string Foo { get; set; }
    
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
        public async Task Option_binding_fails_if_a_required_option_of_non_scalar_type_has_not_been_provided_with_at_least_one_value()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"", IsRequired = true)]
    public IReadOnlyList<string> Foo { get; set; }
    
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
        public async Task Option_binding_fails_if_one_of_the_provided_option_names_is_not_recognized()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"")]
    public string Foo { get; set; }
    
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

        [Fact]
        public async Task Option_binding_fails_if_an_option_of_scalar_type_has_been_provided_with_multiple_values()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption(""foo"")]
    public string Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}");

            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"--foo", "one", "two", "three"},
                new Dictionary<string, string>()
            );

            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().Contain("expects a single value");
        }
    }
}