using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Utils;
using CliFx.Tests.Utils.Extensions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class ConversionSpecs : SpecsBase
    {
        public ConversionSpecs(ITestOutputHelper testOutput)
            : base(testOutput)
        {
        }

        [Fact]
        public async Task Parameter_or_option_value_can_be_converted_to_a_string()
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

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(Foo);
        return default;
    }
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f", "xyz"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("xyz");
        }

        [Fact]
        public async Task Parameter_or_option_value_can_be_converted_to_an_object()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public object Foo { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(Foo);
        return default;
    }
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f", "xyz"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("xyz");
        }

        [Fact]
        public async Task Parameter_or_option_value_can_be_converted_to_a_boolean()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public bool Foo { get; set; }
    
    [CommandOption('b')]
    public bool Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""Foo = "" + Foo);
        console.Output.WriteLine(""Bar = "" + Bar);

        return default;
    }
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f", "true", "-b", "false"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ConsistOfLines(
                "Foo = True",
                "Bar = False"
            );
        }

        [Fact]
        public async Task Parameter_or_option_value_can_be_converted_to_a_boolean_with_implicit_value()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public bool Foo { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(Foo);
        return default;
    }
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("True");
        }

        [Fact]
        public async Task Parameter_or_option_value_can_be_converted_to_an_integer()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public int Foo { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(Foo);
        return default;
    }
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f", "32"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("32");
        }

        [Fact]
        public async Task Parameter_or_option_value_can_be_converted_to_a_double()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public double Foo { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(Foo.ToString(CultureInfo.InvariantCulture));
        return default;
    }
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f", "32.14"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("32.14");
        }

        [Fact]
        public async Task Parameter_or_option_value_can_be_converted_to_DateTimeOffset()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public DateTimeOffset Foo { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(Foo.ToString(""u"", CultureInfo.InvariantCulture));
        return default;
    }
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f", "1995-04-28Z"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("1995-04-28 00:00:00Z");
        }

        [Fact]
        public async Task Parameter_or_option_value_can_be_converted_to_a_TimeSpan()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public TimeSpan Foo { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(Foo.ToString(null, CultureInfo.InvariantCulture));
        return default;
    }
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f", "12:34:56"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("12:34:56");
        }

        [Fact]
        public async Task Parameter_or_option_value_can_be_converted_to_an_enum()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
public enum CustomEnum { One = 1, Two = 2, Three = 3 }

[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public CustomEnum Foo { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine((int) Foo);
        return default;
    }
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f", "two"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("2");
        }

        [Fact]
        public async Task Parameter_or_option_value_can_be_converted_to_a_nullable_integer()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public int? Foo { get; set; }
    
    [CommandOption('b')]
    public int? Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""Foo = "" + Foo);
        console.Output.WriteLine(""Bar = "" + Bar);

        return default;
    }
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-b", "123"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ConsistOfLines(
                "Foo = ",
                "Bar = 123"
            );
        }

        [Fact]
        public async Task Parameter_or_option_value_can_be_converted_to_a_nullable_enum()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
public enum CustomEnum { One = 1, Two = 2, Three = 3 }

[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public CustomEnum? Foo { get; set; }
    
    [CommandOption('b')]
    public CustomEnum? Bar { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""Foo = "" + (int?) Foo);
        console.Output.WriteLine(""Bar = "" + (int?) Bar);

        return default;
    }
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-b", "two"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ConsistOfLines(
                "Foo = ",
                "Bar = 2"
            );
        }

        [Fact]
        public async Task Parameter_or_option_value_can_be_converted_to_a_type_that_has_a_constructor_accepting_a_string()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
public class CustomType
{
    public string Value { get; }
    
    public CustomType(string value) => Value = value;
}

[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public CustomType Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(Foo.Value);
        return default;
    }
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f", "xyz"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("xyz");
        }

        [Fact]
        public async Task Parameter_or_option_value_can_be_converted_to_a_type_that_has_a_static_parse_method()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
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
public class Command : ICommand
{
    [CommandOption('f')]
    public CustomTypeA Foo { get; set; }
    
    [CommandOption('b')]
    public CustomTypeB Bar { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(""Foo = "" + Foo.Value);
        console.Output.WriteLine(""Bar = "" + Bar.Value);

        return default;
    }
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f", "hello", "-b", "world"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ConsistOfLines(
                "Foo = hello",
                "Bar = world"
            );
        }

        [Fact]
        public async Task Parameter_or_option_value_can_be_converted_using_a_custom_converter()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
public class CustomConverter : BindingConverter<int>
{
    public override int Convert(string rawValue) =>
        rawValue.Length;
}

[Command]
public class Command : ICommand
{
    [CommandOption('f', Converter = typeof(CustomConverter))]
    public int Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine(Foo);
        return default;
    }
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f", "hello world"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Trim().Should().Be("11");
        }

        [Fact]
        public async Task Parameter_or_option_value_can_be_converted_to_an_array_of_strings()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public string[] Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console)
    {
        foreach (var i in Foo)
            console.Output.WriteLine(i);

        return default;
    }
}
");
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
        public async Task Parameter_or_option_value_can_be_converted_to_a_read_only_list_of_strings()
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
}
");
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
        public async Task Parameter_or_option_value_can_be_converted_to_a_list_of_strings()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public List<string> Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console)
    {
        foreach (var i in Foo)
            console.Output.WriteLine(i);

        return default;
    }
}
");
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
        public async Task Parameter_or_option_value_can_be_converted_to_an_array_of_integers()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public int[] Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console)
    {
        foreach (var i in Foo)
            console.Output.WriteLine(i);

        return default;
    }
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f", "1", "13", "27"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            // Assert
            exitCode.Should().Be(0);
            stdOut.Should().ConsistOfLines(
                "1",
                "13",
                "27"
            );
        }

        [Fact]
        public async Task Parameter_or_option_value_conversion_fails_if_the_value_cannot_be_converted_to_the_target_type()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public int Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f", "12.34"},
                new Dictionary<string, string>()
            );

            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Parameter_or_option_value_conversion_fails_if_the_target_type_is_not_supported()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
public class CustomType {}

[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public CustomType Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f", "xyz"},
                new Dictionary<string, string>()
            );

            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().Contain("has an unsupported underlying property type");
        }

        [Fact]
        public async Task Parameter_or_option_value_conversion_fails_if_the_target_non_scalar_type_is_not_supported()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
public class CustomType : IEnumerable<object>
{
    public IEnumerator<object> GetEnumerator() => Enumerable.Empty<object>().GetEnumerator();
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public CustomType Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f", "one", "two"},
                new Dictionary<string, string>()
            );

            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().Contain("has an unsupported underlying property type");
        }

        [Fact]
        public async Task Parameter_or_option_value_conversion_fails_if_one_of_the_validators_fail()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
public class ValidatorA : BindingValidator<int>
{
    public override BindingValidationError Validate(int value) => Ok();
}

public class ValidatorB : BindingValidator<int>
{
    public override BindingValidationError Validate(int value) => Error(""Hello world"");
}

[Command]
public class Command : ICommand
{
    [CommandOption('f', Validators = new[] {typeof(ValidatorA), typeof(ValidatorB)})]
    public int Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f", "12"},
                new Dictionary<string, string>()
            );

            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().Contain("Hello world");
        }

        [Fact]
        public async Task Parameter_or_option_value_conversion_fails_if_the_static_parse_method_throws()
        {
            // Arrange
            var commandType = DynamicCommandBuilder.Compile(
                // language=cs
                @"
public class CustomType
{
    public string Value { get; }
    
    private CustomType(string value) => Value = value;
    
    public static CustomType Parse(string value) => throw new Exception(""Hello world"");
}

[Command]
public class Command : ICommand
{
    [CommandOption('f')]
    public CustomType Foo { get; set; }
    
    public ValueTask ExecuteAsync(IConsole console) => default;
}
");
            var application = new CliApplicationBuilder()
                .AddCommand(commandType)
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"-f", "bar"},
                new Dictionary<string, string>()
            );

            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().Contain("Hello world");
        }
    }
}