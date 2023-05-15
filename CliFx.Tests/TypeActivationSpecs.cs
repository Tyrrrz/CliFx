using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using CliFx.Tests.Utils;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests;

public class TypeActivationSpecs : SpecsBase
{
    public TypeActivationSpecs(ITestOutputHelper testOutput)
        : base(testOutput)
    {
    }

    [Fact]
    public async Task I_can_configure_the_application_to_use_the_default_type_activator_to_initialize_types_through_parameterless_constructors()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            """
            [Command]
            public class Command : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.Output.WriteLine("foo");
                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .UseTypeActivator(new DefaultTypeActivator())
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("foo");
    }

    [Fact]
    public async Task I_can_configure_the_application_to_use_the_default_type_activator_and_get_an_error_if_the_requested_type_does_not_have_a_parameterless_constructor()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            """
            [Command]
            public class Command : ICommand
            {
                public Command(string foo) {}

                public ValueTask ExecuteAsync(IConsole console) => default;
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .UseTypeActivator(new DefaultTypeActivator())
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().NotBe(0);

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().Contain("Failed to create an instance of type");
    }

    [Fact]
    public async Task I_can_configure_the_application_to_use_a_custom_type_activator_to_initialize_types_using_a_delegate()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            """
            [Command]
            public class Command : ICommand
            {
                private readonly string _foo;

                public Command(string foo) => _foo = foo;

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.Output.WriteLine(_foo);
                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .UseTypeActivator(type => Activator.CreateInstance(type, "Hello world")!)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("Hello world");
    }

    [Fact]
    public async Task I_can_configure_the_application_to_use_a_custom_type_activator_to_initialize_types_using_a_service_provider()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            """
            [Command]
            public class Command : ICommand
            {
                private readonly string _foo;

                public Command(string foo) => _foo = foo;

                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.Output.WriteLine(_foo);
                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .UseTypeActivator(commandTypes =>
            {
                var services = new ServiceCollection();

                foreach (var serviceType in commandTypes)
                {
                    services.AddSingleton(
                        serviceType,
                        Activator.CreateInstance(serviceType, "Hello world")!
                    );
                }

                return services.BuildServiceProvider();
            })
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().Be(0);

        var stdOut = FakeConsole.ReadOutputString();
        stdOut.Trim().Should().Be("Hello world");
    }

    [Fact]
    public async Task I_can_configure_the_application_to_use_a_custom_type_activator_and_get_an_error_if_the_requested_type_cannot_be_initialized()
    {
        // Arrange
        var commandType = DynamicCommandBuilder.Compile(
            // language=cs
            """
            [Command]
            public class Command : ICommand
            {
                public ValueTask ExecuteAsync(IConsole console)
                {
                    console.Output.WriteLine("foo");
                    return default;
                }
            }
            """
        );

        var application = new CliApplicationBuilder()
            .AddCommand(commandType)
            .UseConsole(FakeConsole)
            .UseTypeActivator((Type _) => null!)
            .Build();

        // Act
        var exitCode = await application.RunAsync(
            Array.Empty<string>(),
            new Dictionary<string, string>()
        );

        // Assert
        exitCode.Should().NotBe(0);

        var stdErr = FakeConsole.ReadErrorString();
        stdErr.Should().Contain("Failed to create an instance of type");
    }
}