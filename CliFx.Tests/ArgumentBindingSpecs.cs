using System.Threading.Tasks;
using CliFx.Infrastructure;
using CliFx.Tests.Commands;
using CliFx.Tests.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class ArgumentBindingSpecs
    {
        private readonly ITestOutputHelper _output;

        public ArgumentBindingSpecs(ITestOutputHelper output) => _output = output;

        [Fact]
        public async Task Property_annotated_as_an_option_can_be_bound_from_multiple_values_even_if_the_inputs_use_mixed_naming()
        {
            // Arrange
            using var console = new FakeInMemoryConsole();

            var application = new CliApplicationBuilder()
                .AddCommand<WithStringArrayOptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--opt", "foo", "-o", "bar", "--opt", "baz"
            });

            var stdOut = console.ReadOutputString();

            var commandInstance = stdOut.DeserializeJson<WithStringArrayOptionCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new WithStringArrayOptionCommand
            {
                Opt = new[] {"foo", "bar", "baz"}
            });
        }

        [Fact]
        public async Task Property_annotated_as_a_required_option_must_always_be_set()
        {
            // Arrange
            using var console = new FakeInMemoryConsole();

            var application = new CliApplicationBuilder()
                .AddCommand<WithSingleRequiredOptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--opt-a", "foo"
            });

            var stdErr = console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);

            stdErr.Should().NotBeNullOrWhiteSpace();

            _output.WriteLine(stdErr);
        }

        [Fact]
        public async Task Property_annotated_as_a_required_option_must_always_be_bound_to_some_value()
        {
            // Arrange
            using var console = new FakeInMemoryConsole();

            var application = new CliApplicationBuilder()
                .AddCommand<WithSingleRequiredOptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--opt-a"
            });

            var stdErr = console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);

            stdErr.Should().NotBeNullOrWhiteSpace();

            _output.WriteLine(stdErr);
        }

        [Fact]
        public async Task Property_annotated_as_a_required_option_must_always_be_bound_to_at_least_one_value_if_it_expects_multiple_values()
        {
            // Arrange
            using var console = new FakeInMemoryConsole();

            var application = new CliApplicationBuilder()
                .AddCommand<WithRequiredOptionsCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--opt-a", "foo"
            });

            var stdErr = console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);

            stdErr.Should().NotBeNullOrWhiteSpace();

            _output.WriteLine(stdErr);
        }

        [Fact]
        public async Task Property_annotated_as_parameter_is_bound_directly_from_argument_value_according_to_the_order()
        {
            // Arrange
            using var console = new FakeInMemoryConsole();

            var application = new CliApplicationBuilder()
                .AddCommand<WithParametersCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "foo", "13", "bar", "baz"
            });

            var commandInstance = console.ReadOutputString().DeserializeJson<WithParametersCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new WithParametersCommand
            {
                ParamA = "foo",
                ParamB = 13,
                ParamC = new[] {"bar", "baz"}
            });
        }

        [Fact]
        public async Task Property_annotated_as_parameter_must_always_be_bound_to_some_value()
        {
            // Arrange
            using var console = new FakeInMemoryConsole();

            var application = new CliApplicationBuilder()
                .AddCommand<WithSingleParameterCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd"
            });

            var stdErr = console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);

            stdErr.Should().NotBeNullOrWhiteSpace();

            _output.WriteLine(stdErr);
        }

        [Fact]
        public async Task Property_annotated_as_parameter_must_always_be_bound_to_at_least_one_value_if_it_expects_multiple_values()
        {
            // Arrange
            using var console = new FakeInMemoryConsole();

            var application = new CliApplicationBuilder()
                .AddCommand<WithParametersCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "foo", "13"
            });

            var stdErr = console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);

            stdErr.Should().NotBeNullOrWhiteSpace();

            _output.WriteLine(stdErr);
        }

        [Fact]
        public async Task Argument_that_begins_with_a_dash_is_not_parsed_as_option_name_if_it_does_not_start_with_a_letter_character()
        {
            // Arrange
            using var console = new FakeInMemoryConsole();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--int", "-13"
            });

            var commandInstance = console.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);

            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Int = -13
            });
        }

        [Fact]
        public async Task All_provided_option_arguments_must_be_bound_to_corresponding_properties()
        {
            // Arrange
            using var console = new FakeInMemoryConsole();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--non-existing-option", "13"
            });

            var stdErr = console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);

            stdErr.Should().NotBeNullOrWhiteSpace();

            _output.WriteLine(stdErr);
        }

        [Fact]
        public async Task All_provided_parameter_arguments_must_be_bound_to_corresponding_properties()
        {
            // Arrange
            using var console = new FakeInMemoryConsole();

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cnd", "non-existing-parameter"
            });

            var stdErr = console.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);

            stdErr.Should().NotBeNullOrWhiteSpace();

            _output.WriteLine(stdErr);
        }
    }
}