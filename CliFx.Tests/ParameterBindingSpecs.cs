using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Commands;
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
        public async Task Parameter_is_bound_directly_from_argument_value_according_to_the_order()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<WithParametersCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "foo", "13", "bar", "baz"},
                new Dictionary<string, string>()
            );

            var commandInstance = FakeConsole.ReadOutputString().DeserializeJson<WithParametersCommand>();

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
        public async Task Binding_fails_if_one_of_the_parameters_has_not_been_provided()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<WithSingleParameterCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd"},
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
            var application = new CliApplicationBuilder()
                .AddCommand<WithParametersCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "foo", "13"},
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
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cnd", "non-existing-parameter"},
                new Dictionary<string, string>()
            );

            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().Contain("Unrecognized parameters provided");
        }
    }
}