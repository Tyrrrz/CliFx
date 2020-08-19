using System.IO;
using System.Threading.Tasks;
using CliFx.Exceptions;
using CliFx.Tests.Commands;
using FluentAssertions;
using Newtonsoft.Json;
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
            await using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut);

            var application = new CliApplicationBuilder()
                .AddCommand<WithStringArrayOptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--opt", "foo", "-o", "bar", "--opt", "baz"
            });

            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();
            var commandInstance = JsonConvert.DeserializeObject<WithStringArrayOptionCommand>(stdOutData);

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
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<WithSingleRequiredOptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--opt-a", "foo"
            });

            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Property_annotated_as_a_required_option_must_always_be_bound_to_some_value()
        {
            // Arrange
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<WithSingleRequiredOptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--opt-a"
            });

            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Property_annotated_as_a_required_option_must_always_be_bound_to_at_least_one_value_if_it_expects_multiple_values()
        {
            // Arrange
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<WithRequiredOptionsCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--opt-a", "foo"
            });

            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Property_annotated_as_parameter_is_bound_directly_from_argument_value_according_to_the_order()
        {
            // Arrange
            await using var stdOut = new MemoryStream();
            var console = new VirtualConsole(output: stdOut);

            var application = new CliApplicationBuilder()
                .AddCommand<WithParametersCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "foo", "13", "bar", "baz"
            });

            var stdOutData = console.Output.Encoding.GetString(stdOut.ToArray()).TrimEnd();
            var commandInstance = JsonConvert.DeserializeObject<WithParametersCommand>(stdOutData);

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
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<WithSingleParameterCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd"
            });

            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Property_annotated_as_parameter_must_always_be_bound_to_at_least_one_value_if_it_expects_multiple_values()
        {
            // Arrange
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<WithParametersCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "foo", "13"
            });

            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task All_provided_option_arguments_must_be_bound_to_corresponding_properties()
        {
            // Arrange
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cmd", "--non-existing-option", "13"
            });

            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task All_provided_parameter_arguments_must_be_bound_to_corresponding_properties()
        {
            // Arrange
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(new[]
            {
                "cnd", "non-existing-parameter"
            });

            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }
    }
}