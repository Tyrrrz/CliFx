using System;
using System.IO;
using CliFx.Domain;
using CliFx.Exceptions;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public partial class ApplicationSpecs
    {
        private readonly ITestOutputHelper _output;

        public ApplicationSpecs(ITestOutputHelper output) => _output = output;

        [Fact]
        public void Application_can_be_created_with_a_default_configuration()
        {
            // Act
            var app = new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .Build();

            // Assert
            app.Should().NotBeNull();
        }

        [Fact]
        public void Application_can_be_created_with_a_custom_configuration()
        {
            // Act
            var app = new CliApplicationBuilder()
                .AddCommand(typeof(ValidCommand))
                .AddCommandsFrom(typeof(ValidCommand).Assembly)
                .AddCommands(new[] {typeof(ValidCommand)})
                .AddCommandsFrom(new[] {typeof(ValidCommand).Assembly})
                .AddCommandsFromThisAssembly()
                .AllowDebugMode()
                .AllowPreviewMode()
                .UseTitle("test")
                .UseExecutableName("test")
                .UseVersionText("test")
                .UseDescription("test")
                .UseConsole(new VirtualConsole(Stream.Null))
                .UseTypeActivator(Activator.CreateInstance)
                .Build();

            // Assert
            app.Should().NotBeNull();
        }

        [Fact]
        public void At_least_one_command_must_be_defined_in_an_application()
        {
            // Arrange
            var commandTypes = Array.Empty<Type>();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Commands_must_implement_the_corresponding_interface()
        {
            // Arrange
            var commandTypes = new[] {typeof(NonImplementedCommand)};

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Commands_must_be_annotated_by_an_attribute()
        {
            // Arrange
            var commandTypes = new[] {typeof(NonAnnotatedCommand)};

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Commands_must_have_unique_names()
        {
            // Arrange
            var commandTypes = new[] {typeof(DuplicateNameCommandA), typeof(DuplicateNameCommandB)};

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_parameters_must_have_unique_order()
        {
            // Arrange
            var commandTypes = new[] {typeof(DuplicateParameterOrderCommand)};

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_parameters_must_have_unique_names()
        {
            // Arrange
            var commandTypes = new[] {typeof(DuplicateParameterNameCommand)};

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_parameter_can_be_non_scalar_only_if_no_other_such_parameter_is_present()
        {
            // Arrange
            var commandTypes = new[] {typeof(MultipleNonScalarParametersCommand)};

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_parameter_can_be_non_scalar_only_if_it_is_the_last_in_order()
        {
            // Arrange
            var commandTypes = new[] {typeof(NonLastNonScalarParameterCommand)};

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_options_must_have_names_that_are_not_empty()
        {
            // Arrange
            var commandTypes = new[] {typeof(EmptyOptionNameCommand)};

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_options_must_have_names_that_are_longer_than_one_character()
        {
            // Arrange
            var commandTypes = new[] {typeof(SingleCharacterOptionNameCommand)};

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_options_must_have_unique_names()
        {
            // Arrange
            var commandTypes = new[] {typeof(DuplicateOptionNamesCommand)};

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_options_must_have_unique_short_names()
        {
            // Arrange
            var commandTypes = new[] {typeof(DuplicateOptionShortNamesCommand)};

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_options_must_have_unique_environment_variable_names()
        {
            // Arrange
            var commandTypes = new[] {typeof(DuplicateOptionEnvironmentVariableNamesCommand)};

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_options_and_parameters_must_be_annotated_by_corresponding_attributes()
        {
            // Arrange
            var commandTypes = new[] {typeof(HiddenPropertiesCommand)};

            // Act
            var schema = ApplicationSchema.Resolve(commandTypes);

            // Assert
            schema.Should().BeEquivalentTo(new ApplicationSchema(new[]
            {
                new CommandSchema(
                    typeof(HiddenPropertiesCommand),
                    "hidden",
                    "Description",
                    new[]
                    {
                        new CommandParameterSchema(
                            typeof(HiddenPropertiesCommand).GetProperty(nameof(HiddenPropertiesCommand.Parameter)),
                            13,
                            "param",
                            "Param description")
                    },
                    new[]
                    {
                        new CommandOptionSchema(
                            typeof(HiddenPropertiesCommand).GetProperty(nameof(HiddenPropertiesCommand.Option)),
                            "option",
                            'o',
                            "ENV",
                            false,
                            "Option description")
                    })
            }));

            schema.ToString().Should().NotBeNullOrWhiteSpace(); // this is only for coverage, I'm sorry
        }
    }
}