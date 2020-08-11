using System;
using System.Collections.Generic;
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
                .AddCommand(typeof(DefaultCommand))
                .AddCommandsFrom(typeof(DefaultCommand).Assembly)
                .AddCommands(new[] { typeof(DefaultCommand) })
                .AddCommandsFrom(new[] { typeof(DefaultCommand).Assembly })
                .AddCommandsFromThisAssembly()
                .AllowDebugMode()
                .AllowPreviewMode()
                .UseTitle("test")
                .UseExecutableName("test")
                .UseVersionText("test")
                .UseDescription("test")
                .UseConsole(new VirtualConsole(Stream.Null))
                .UseTypeActivator(Activator.CreateInstance!)
                .Build();

            // Assert
            app.Should().NotBeNull();
        }

        [Fact]
        public void At_least_one_command_must_be_defined_in_an_application()
        {
            // Arrange
            var commandTypes = Array.Empty<Type>();
            var directiveTypes = Array.Empty<Type>();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => RootSchema.Resolve(commandTypes, directiveTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Commands_must_implement_the_corresponding_interface()
        {
            // Arrange
            var commandTypes = new[] { typeof(NonImplementedCommand) };
            var directiveTypes = Array.Empty<Type>();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => RootSchema.Resolve(commandTypes, directiveTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Commands_must_be_annotated_by_an_attribute()
        {
            // Arrange
            var commandTypes = new[] { typeof(NonAnnotatedCommand) };
            var directiveTypes = Array.Empty<Type>();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => RootSchema.Resolve(commandTypes, directiveTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Commands_must_have_unique_names()
        {
            // Arrange
            var commandTypes = new[] { typeof(DuplicateNameCommandA), typeof(DuplicateNameCommandB) };
            var directiveTypes = Array.Empty<Type>();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => RootSchema.Resolve(commandTypes, directiveTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_can_be_default_but_only_if_it_is_the_only_such_command()
        {
            // Arrange
            var commandTypes = new[] { typeof(DefaultCommand), typeof(AnotherDefaultCommand) };
            var directiveTypes = Array.Empty<Type>();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => RootSchema.Resolve(commandTypes, directiveTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_parameters_must_have_unique_order()
        {
            // Arrange
            var commandTypes = new[] { typeof(DuplicateParameterOrderCommand) };
            var directiveTypes = Array.Empty<Type>();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => RootSchema.Resolve(commandTypes, directiveTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_parameters_must_have_unique_names()
        {
            // Arrange
            var commandTypes = new[] { typeof(DuplicateParameterNameCommand) };
            var directiveTypes = Array.Empty<Type>();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => RootSchema.Resolve(commandTypes, directiveTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_parameter_can_be_non_scalar_only_if_no_other_such_parameter_is_present()
        {
            // Arrange
            var commandTypes = new[] { typeof(MultipleNonScalarParametersCommand) };
            var directiveTypes = Array.Empty<Type>();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => RootSchema.Resolve(commandTypes, directiveTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_parameter_can_be_non_scalar_only_if_it_is_the_last_in_order()
        {
            // Arrange
            var commandTypes = new[] { typeof(NonLastNonScalarParameterCommand) };
            var directiveTypes = Array.Empty<Type>();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => RootSchema.Resolve(commandTypes, directiveTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_options_must_have_names_that_are_not_empty()
        {
            // Arrange
            var commandTypes = new[] { typeof(EmptyOptionNameCommand) };
            var directiveTypes = Array.Empty<Type>();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => RootSchema.Resolve(commandTypes, directiveTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_options_must_have_names_that_are_longer_than_one_character()
        {
            // Arrange
            var commandTypes = new[] { typeof(SingleCharacterOptionNameCommand) };
            var directiveTypes = Array.Empty<Type>();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => RootSchema.Resolve(commandTypes, directiveTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_options_must_have_unique_names()
        {
            // Arrange
            var commandTypes = new[] { typeof(DuplicateOptionNamesCommand) };
            var directiveTypes = Array.Empty<Type>();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => RootSchema.Resolve(commandTypes, directiveTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_options_must_have_unique_short_names()
        {
            // Arrange
            var commandTypes = new[] { typeof(DuplicateOptionShortNamesCommand) };
            var directiveTypes = Array.Empty<Type>();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => RootSchema.Resolve(commandTypes, directiveTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_options_must_not_have_conflicts_with_the_implicit_help_option()
        {
            // Arrange
            var commandTypes = new[] { typeof(ConflictWithHelpOptionCommand) };
            var directiveTypes = Array.Empty<Type>();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => RootSchema.Resolve(commandTypes, directiveTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_options_must_not_have_conflicts_with_the_implicit_version_option()
        {
            // Arrange
            var commandTypes = new[] { typeof(ConflictWithVersionOptionCommand) };
            var directiveTypes = Array.Empty<Type>();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => RootSchema.Resolve(commandTypes, directiveTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_options_must_have_unique_environment_variable_names()
        {
            // Arrange
            var commandTypes = new[] { typeof(DuplicateOptionEnvironmentVariableNamesCommand) };
            var directiveTypes = Array.Empty<Type>();

            // Act & assert
            var ex = Assert.Throws<CliFxException>(() => RootSchema.Resolve(commandTypes, directiveTypes));
            _output.WriteLine(ex.Message);
        }

        [Fact]
        public void Command_options_and_parameters_must_be_annotated_by_corresponding_attributes()
        {
            // Arrange
            var commandTypes = new[] { typeof(HiddenPropertiesCommand) };
            var directiveTypes = Array.Empty<Type>();

            // Act
            var schema = RootSchema.Resolve(commandTypes, directiveTypes);

            // Assert
            schema.Should().BeEquivalentTo(new RootSchema(new Dictionary<string, DirectiveSchema>(),
                new Dictionary<string, CommandSchema>
                {
                    { "hidden",
                      new CommandSchema(
                          typeof(HiddenPropertiesCommand),
                          "hidden",
                          "Description",
                          "Manual",
                          false,
                          new[]
                          {
                              new CommandParameterSchema(
                                  typeof(HiddenPropertiesCommand).GetProperty(nameof(HiddenPropertiesCommand.Parameter))!,
                                  13,
                                  "param",
                                  "Param description")
                          },
                          new[]
                          {
                              new CommandOptionSchema(
                                  typeof(HiddenPropertiesCommand).GetProperty(nameof(HiddenPropertiesCommand.Option))!,
                                  "option",
                                  'o',
                                  "ENV",
                                  false,
                                  "Option description"),
                              CommandOptionSchema.HelpOption
                          })
                    }
                }, null));

            schema.ToString().Should().NotBeNullOrWhiteSpace(); // this is only for coverage, I'm sorry
        }
    }
}