using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Tests.Commands;
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
            var application = new CliApplicationBuilder()
                .AddCommand<WithStringArrayOptionCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "--opt", "foo", "-o", "bar", "--opt", "baz"},
                new Dictionary<string, string>()
            );

            var stdOut = FakeConsole.ReadOutputString();

            var commandInstance = stdOut.DeserializeJson<WithStringArrayOptionCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new WithStringArrayOptionCommand
            {
                Opt = new[] {"foo", "bar", "baz"}
            });
        }

        [Fact]
        public async Task Argument_that_begins_with_a_dash_followed_by_a_non_letter_character_is_parsed_as_a_value()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "--int", "-13"},
                new Dictionary<string, string>()
            );

            var commandInstance = FakeConsole.ReadOutputString().DeserializeJson<SupportedArgumentTypesCommand>();

            // Assert
            exitCode.Should().Be(0);
            commandInstance.Should().BeEquivalentTo(new SupportedArgumentTypesCommand
            {
                Int = -13
            });
        }

        [Fact]
        public async Task Binding_fails_if_a_required_option_has_not_been_provided()
        {
            // Arrange
            var application = new CliApplicationBuilder()
                .AddCommand<WithSingleRequiredOptionCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "--opt-a", "foo"},
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
            var application = new CliApplicationBuilder()
                .AddCommand<WithSingleRequiredOptionCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "--opt-b"},
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
            var application = new CliApplicationBuilder()
                .AddCommand<WithRequiredOptionsCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "--opt-a", "foo"},
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
            var application = new CliApplicationBuilder()
                .AddCommand<SupportedArgumentTypesCommand>()
                .UseConsole(FakeConsole)
                .Build();

            // Act
            var exitCode = await application.RunAsync(
                new[] {"cmd", "--non-existing-option", "13"},
                new Dictionary<string, string>()
            );

            var stdErr = FakeConsole.ReadErrorString();

            // Assert
            exitCode.Should().NotBe(0);
            stdErr.Should().Contain("Unrecognized options provided");
        }
    }
}