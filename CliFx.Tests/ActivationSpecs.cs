using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public partial class ActivationSpecs(ITestOutputHelper testOutput) : SpecsBase(testOutput)
{
    [Fact]
    public async Task I_can_pass_a_value_to_an_input_bound_to_a_string_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public string? Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            console.WriteLine(Foo);
                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["-f", "xyz"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("xyz");
    }

    [Fact]
    public async Task I_can_pass_a_value_to_an_input_bound_to_an_object_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public object? Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            console.WriteLine(Foo);
                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["-f", "xyz"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("xyz");
    }

    [Fact]
    public async Task I_can_pass_a_value_to_an_input_bound_to_a_boolean_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public bool Foo { get; set; }

                        [CommandOption('b')]
                        public bool Bar { get; set; }

                        [CommandOption('c')]
                        public bool Baz { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            console.WriteLine("Foo = " + Foo);
                            console.WriteLine("Bar = " + Bar);
                            console.WriteLine("Baz = " + Baz);

                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-f", "true", "-b", "false", "-c"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("Foo = True", "Bar = False", "Baz = True");
    }

    [Fact]
    public async Task I_can_pass_a_value_to_an_input_bound_to_a_boolean_property_using_alternative_identifiers()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('a')]
                        public bool A { get; set; }

                        [CommandOption('b')]
                        public bool B { get; set; }

                        [CommandOption('c')]
                        public bool C { get; set; }

                        [CommandOption('d')]
                        public bool D { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            console.WriteLine("A = " + A);
                            console.WriteLine("B = " + B);
                            console.WriteLine("C = " + C);
                            console.WriteLine("D = " + D);

                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-a", "on", "-b", "off", "-c", "yes", "-d", "no"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("A = True", "B = False", "C = True", "D = False");
    }

    [Fact]
    public async Task I_can_pass_a_value_to_an_input_bound_to_an_integer_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public int Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            console.WriteLine(Foo);
                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["-f", "32"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("32");
    }

    [Fact]
    public async Task I_can_pass_a_value_to_an_input_bound_to_a_double_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public double Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            console.WriteLine(Foo.ToString(CultureInfo.InvariantCulture));
                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-f", "32.14"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("32.14");
    }

    [Fact]
    public async Task I_can_pass_a_value_to_an_input_bound_to_a_DateTimeOffset_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public DateTimeOffset Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            console.WriteLine(Foo.ToString("u", CultureInfo.InvariantCulture));
                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-f", "1995-04-28Z"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("1995-04-28 00:00:00Z");
    }

    [Fact]
    public async Task I_can_pass_a_value_to_an_input_bound_to_a_TimeSpan_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public TimeSpan Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            console.WriteLine(Foo.ToString(null, CultureInfo.InvariantCulture));
                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-f", "12:34:56"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("12:34:56");
    }

    [Fact]
    public async Task I_can_pass_a_value_to_an_input_bound_to_an_enum_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    public enum CustomEnum { One = 1, Two = 2, Three = 3 }

                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public CustomEnum Foo { get; set; }

                        [CommandOption('b')]
                        public CustomEnum Bar { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            console.WriteLine("Foo = " + (int) Foo);
                            console.WriteLine("Bar = " + (int) Bar);
                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-f", "two", "-b", "2"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("Foo = 2", "Bar = 2");
    }

    [Fact]
    public async Task I_can_pass_a_value_to_an_input_bound_to_a_nullable_integer_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public int? Foo { get; set; }

                        [CommandOption('b')]
                        public int? Bar { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            console.WriteLine("Foo = " + Foo);
                            console.WriteLine("Bar = " + Bar);

                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["-b", "123"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("Foo = ", "Bar = 123");
    }

    [Fact]
    public async Task I_can_pass_a_value_to_an_input_bound_to_a_nullable_enum_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    public enum CustomEnum { One = 1, Two = 2, Three = 3 }

                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public CustomEnum? Foo { get; set; }

                        [CommandOption('b')]
                        public CustomEnum? Bar { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            console.WriteLine("Foo = " + (int?) Foo);
                            console.WriteLine("Bar = " + (int?) Bar);

                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["-b", "two"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("Foo = ", "Bar = 2");
    }

    [Fact]
    public async Task I_can_pass_a_value_to_an_input_bound_to_a_string_parsable_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    public class CustomTypeA
                    {
                        public string Value { get; }

                        private CustomTypeA(string value) => Value = value;

                        public static CustomTypeA Parse(string value) =>
                            new CustomTypeA(value);
                    }

                    public class CustomTypeB
                    {
                        public string Value { get; }

                        private CustomTypeB(string value) => Value = value;

                        public static CustomTypeB Parse(string value, IFormatProvider formatProvider) =>
                            new CustomTypeB(value);
                    }

                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public CustomTypeA? Foo { get; set; }

                        [CommandOption('b')]
                        public CustomTypeB? Bar { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            console.WriteLine("Foo = " + Foo.Value);
                            console.WriteLine("Bar = " + Bar.Value);

                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-f", "hello", "-b", "world"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("Foo = hello", "Bar = world");
    }

    [Fact]
    public async Task I_can_pass_a_value_to_an_input_bound_to_a_string_constructible_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    public class CustomType
                    {
                        public string Value { get; }

                        public CustomType(string value) => Value = value;
                    }

                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public CustomType? Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            console.WriteLine(Foo.Value);
                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["-f", "xyz"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("xyz");
    }

    [Fact]
    public async Task I_can_pass_values_to_an_input_bound_to_a_string_array_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public string[]? Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            foreach (var i in Foo)
                                console.WriteLine(i);

                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-f", "one", "two", "three"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("one", "two", "three");
    }

    [Fact]
    public async Task I_can_pass_values_to_an_input_bound_to_a_read_only_list_of_strings_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public IReadOnlyList<string>? Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            foreach (var i in Foo)
                                console.WriteLine(i);

                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-f", "one", "two", "three"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("one", "two", "three");
    }

    [Fact]
    public async Task I_can_pass_values_to_an_input_bound_to_a_collection_of_strings_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public ICollection<string>? Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            foreach (var i in Foo)
                                console.WriteLine(i);

                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-f", "one", "two", "three"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("one", "two", "three");
    }

    [Fact]
    public async Task I_can_pass_values_to_an_input_bound_to_a_string_list_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public List<string>? Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            foreach (var i in Foo)
                                console.WriteLine(i);

                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-f", "one", "two", "three"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("one", "two", "three");
    }

    [Fact]
    public async Task I_can_pass_values_to_an_input_bound_to_an_integer_array_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public int[]? Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            foreach (var i in Foo)
                                console.WriteLine(i);

                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-f", "1", "13", "27"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("1", "13", "27");
    }

    [Fact]
    public async Task I_can_pass_values_to_an_input_bound_to_a_read_only_list_of_integers_property()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public IReadOnlyList<int>? Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            foreach (var i in Foo)
                                console.WriteLine(i);

                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-f", "1", "13", "27"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Should().ConsistOfLines("1", "13", "27");
    }

    [Fact]
    public async Task I_can_pass_a_value_to_an_input_bound_to_a_property_with_a_custom_converter()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    public class CustomConverter : ScalarInputConverter<int>
                    {
                        public override int Convert(string rawValue) =>
                            rawValue.Length;
                    }

                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f', Converter = typeof(CustomConverter))]
                        public int Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console)
                        {
                            console.WriteLine(Foo);
                            return default;
                        }
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-f", "hello world"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("11");
    }

    [Fact]
    public async Task I_can_try_to_pass_an_invalid_value_to_an_input_and_get_an_error()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public int Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console) => default;
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            ["-f", "12.34"],
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().NotBe(0);

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task I_can_try_to_pass_a_value_to_an_input_bound_to_a_property_with_a_custom_validator_and_get_an_error_if_validation_fails()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    public class ValidatorA : InputValidator<int>
                    {
                        public override InputValidationError Validate(int value) => Ok();
                    }

                    public class ValidatorB : InputValidator<int>
                    {
                        public override InputValidationError Validate(int value) => Error("Hello world");
                    }

                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f', Validators = [typeof(ValidatorA), typeof(ValidatorB)])]
                        public int Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console) => default;
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["-f", "12"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().NotBe(0);

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().Contain("Hello world");
    }

    [Fact]
    public async Task I_can_try_to_pass_a_value_to_an_input_bound_to_a_string_parsable_property_and_get_an_error_if_parsing_fails()
    {
        // Arrange
        var application = new CommandLineApplicationBuilder()
            .AddCommands(
                CommandCompiler.Compile(
                    // lang=csharp
                    """
                    public class CustomType
                    {
                        public string Value { get; }

                        private CustomType(string value) => Value = value;

                        public static CustomType Parse(string value) => throw new Exception("Hello world");
                    }

                    [Command]
                    public partial class Command : ICommand
                    {
                        [CommandOption('f')]
                        public CustomType? Foo { get; set; }

                        public ValueTask ExecuteAsync(IConsole console) => default;
                    }
                    """
                )
            )
            .UseConsole(FakeConsole)
            .Build();

        // Act
        var exitCode = await application.RunAsync(["-f", "bar"], new Dictionary<string, string>());

        // Assert
        exitCode.Should().NotBe(0);

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().Contain("Hello world");
    }
}
