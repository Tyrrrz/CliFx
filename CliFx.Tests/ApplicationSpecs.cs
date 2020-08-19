using System;
using System.IO;
using System.Threading.Tasks;
using CliFx.Tests.Commands;
using CliFx.Tests.Commands.Invalid;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CliFx.Tests
{
    public class ApplicationSpecs
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
                .AddCommand<DefaultCommand>()
                .AddCommandsFrom(typeof(DefaultCommand).Assembly)
                .AddCommands(new[] {typeof(DefaultCommand)})
                .AddCommandsFrom(new[] {typeof(DefaultCommand).Assembly})
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
        public async Task At_least_one_command_must_be_defined_in_an_application()
        {
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Commands_must_implement_the_corresponding_interface()
        {
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand(typeof(NonImplementedCommand))
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Commands_must_be_annotated_by_an_attribute()
        {
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<NonAnnotatedCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Commands_must_have_unique_names()
        {
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<GenericExceptionCommand>()
                .AddCommand<CommandExceptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_can_be_default_but_only_if_it_is_the_only_such_command()
        {
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<DefaultCommand>()
                .AddCommand<OtherDefaultCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_parameters_must_have_unique_order()
        {
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<DuplicateParameterOrderCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_parameters_must_have_unique_names()
        {
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<DuplicateParameterNameCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_parameter_can_be_non_scalar_only_if_no_other_such_parameter_is_present()
        {
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<MultipleNonScalarParametersCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_parameter_can_be_non_scalar_only_if_it_is_the_last_in_order()
        {
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<NonLastNonScalarParameterCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_options_must_have_names_that_are_not_empty()
        {
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<EmptyOptionNameCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_options_must_have_names_that_are_longer_than_one_character()
        {
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<SingleCharacterOptionNameCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_options_must_have_unique_names()
        {
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<DuplicateOptionNamesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_options_must_have_unique_short_names()
        {
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<DuplicateOptionShortNamesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_options_must_have_unique_environment_variable_names()
        {
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<DuplicateOptionEnvironmentVariableNamesCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_options_must_not_have_conflicts_with_the_implicit_help_option()
        {
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<ConflictWithHelpOptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }

        [Fact]
        public async Task Command_options_must_not_have_conflicts_with_the_implicit_version_option()
        {
            await using var stdErr = new MemoryStream();
            var console = new VirtualConsole(error: stdErr);

            var application = new CliApplicationBuilder()
                .AddCommand<ConflictWithVersionOptionCommand>()
                .UseConsole(console)
                .Build();

            // Act
            var exitCode = await application.RunAsync(Array.Empty<string>());
            var stdErrData = console.Error.Encoding.GetString(stdErr.ToArray()).TrimEnd();

            // Assert
            exitCode.Should().NotBe(0);
            stdErrData.Should().NotBeEmpty();

            _output.WriteLine(stdErrData);
        }
    }
}