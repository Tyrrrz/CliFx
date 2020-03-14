﻿using System;
using CliFx.Domain;
using CliFx.Exceptions;
using CliFx.Tests.TestCommands;
using FluentAssertions;
using Xunit;

namespace CliFx.Tests
{
    public partial class ApplicationSpecs
    {
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
                .AddCommand(typeof(HelloWorldDefaultCommand))
                .AddCommandsFrom(typeof(HelloWorldDefaultCommand).Assembly)
                .AddCommands(new[] {typeof(HelloWorldDefaultCommand)})
                .AddCommandsFrom(new[] {typeof(HelloWorldDefaultCommand).Assembly})
                .AddCommandsFromThisAssembly()
                .AllowDebugMode()
                .AllowPreviewMode()
                .UseTitle("test")
                .UseExecutableName("test")
                .UseVersionText("test")
                .UseDescription("test")
                .UseConsole(new VirtualConsole())
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
            Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
        }

        [Fact]
        public void Commands_must_implement_the_command_interface()
        {
            // Arrange
            var commandTypes = new[] {typeof(NonImplementedCommand)};

            // Act & assert
            Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
        }

        [Fact]
        public void Commands_must_be_annotated_by_an_attribute()
        {
            // Arrange
            var commandTypes = new[] {typeof(NonAnnotatedCommand)};

            // Act & assert
            Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
        }

        [Fact]
        public void Commands_must_have_unique_names()
        {
            // Arrange
            var commandTypes = new[] {typeof(DuplicateNameCommandA), typeof(DuplicateNameCommandB)};

            // Act & assert
            Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
        }

        [Fact]
        public void Command_parameters_must_have_unique_order()
        {
            // Arrange
            var commandTypes = new[] {typeof(DuplicateParameterOrderCommand)};

            // Act & assert
            Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
        }

        [Fact]
        public void Command_parameters_must_have_unique_names()
        {
            // Arrange
            var commandTypes = new[] {typeof(DuplicateParameterNameCommand)};

            // Act & assert
            Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
        }

        [Fact]
        public void Command_parameter_can_be_non_scalar_only_if_no_other_such_parameter_is_present()
        {
            // Arrange
            var commandTypes = new[] {typeof(MultipleNonScalarParametersCommand)};

            // Act & assert
            Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
        }

        [Fact]
        public void Command_parameter_can_be_non_scalar_only_if_it_is_the_last_in_order()
        {
            // Arrange
            var commandTypes = new[] {typeof(NonLastNonScalarParameterCommand)};

            // Act & assert
            Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
        }

        [Fact]
        public void Command_options_must_have_unique_names()
        {
            // Arrange
            var commandTypes = new[] {typeof(DuplicateOptionNamesCommand)};

            // Act & assert
            Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
        }

        [Fact]
        public void Command_options_must_have_unique_short_names()
        {
            // Arrange
            var commandTypes = new[] {typeof(DuplicateOptionShortNamesCommand)};

            // Act & assert
            Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
        }

        [Fact]
        public void Command_options_must_have_unique_environment_variable_names()
        {
            // Arrange
            var commandTypes = new[] {typeof(DuplicateOptionEnvironmentVariableNamesCommand)};

            // Act & assert
            Assert.Throws<CliFxException>(() => ApplicationSchema.Resolve(commandTypes));
        }
    }
}