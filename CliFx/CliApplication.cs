using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Exceptions;
using CliFx.Internal;
using CliFx.Models;
using CliFx.Services;

namespace CliFx
{
    /// <summary>
    /// Default implementation of <see cref="ICliApplication"/>.
    /// </summary>
    public class CliApplication : ICliApplication
    {
        private readonly ApplicationMetadata _metadata;
        private readonly ApplicationConfiguration _configuration;

        private readonly IConsole _console;
        private readonly ICommandInputParser _commandInputParser;
        private readonly ICommandSchemaResolver _commandSchemaResolver;
        private readonly ICommandFactory _commandFactory;
        private readonly ICommandInitializer _commandInitializer;
        private readonly IHelpTextRenderer _helpTextRenderer;

        /// <summary>
        /// Initializes an instance of <see cref="CliApplication"/>.
        /// </summary>
        public CliApplication(ApplicationMetadata metadata, ApplicationConfiguration configuration,
            IConsole console, ICommandInputParser commandInputParser, ICommandSchemaResolver commandSchemaResolver,
            ICommandFactory commandFactory, ICommandInitializer commandInitializer, IHelpTextRenderer helpTextRenderer)
        {
            _metadata = metadata.GuardNotNull(nameof(metadata));
            _configuration = configuration.GuardNotNull(nameof(configuration));

            _console = console.GuardNotNull(nameof(console));
            _commandInputParser = commandInputParser.GuardNotNull(nameof(commandInputParser));
            _commandSchemaResolver = commandSchemaResolver.GuardNotNull(nameof(commandSchemaResolver));
            _commandFactory = commandFactory.GuardNotNull(nameof(commandFactory));
            _commandInitializer = commandInitializer.GuardNotNull(nameof(commandInitializer));
            _helpTextRenderer = helpTextRenderer.GuardNotNull(nameof(helpTextRenderer));
        }

        /// <inheritdoc />
        public async Task<int> RunAsync(IReadOnlyList<string> commandLineArguments)
        {
            commandLineArguments.GuardNotNull(nameof(commandLineArguments));

            try
            {
                // Get schemas for all available command types
                var availableCommandSchemas = _commandSchemaResolver.GetCommandSchemas(_configuration.CommandTypes);

                // Parse command input from arguments
                var commandInput = _commandInputParser.ParseCommandInput(commandLineArguments);

                // Find command schema matching the name specified in the input
                var targetCommandSchema = availableCommandSchemas.FindByName(commandInput.CommandName);

                // Handle cases where requested command is not defined
                if (targetCommandSchema == null)
                {
                    var isError = false;

                    // If specified a command - show error
                    if (commandInput.IsCommandSpecified())
                    {
                        isError = true;

                        _console.WithForegroundColor(ConsoleColor.Red,
                            () => _console.Error.WriteLine($"Specified command [{commandInput.CommandName}] is not defined."));
                    }

                    // Get parent command schema
                    var parentCommandSchema = availableCommandSchemas.FindParent(commandInput.CommandName);

                    // Show help for parent command if it's defined
                    if (parentCommandSchema != null)
                    {
                        var helpTextSource = new HelpTextSource(_metadata, availableCommandSchemas, parentCommandSchema);
                        _helpTextRenderer.RenderHelpText(_console, helpTextSource);
                    }
                    // Otherwise show help for a stub default command
                    else
                    {
                        var helpTextSource = new HelpTextSource(_metadata,
                            availableCommandSchemas.Concat(CommandSchema.StubDefaultCommand).ToArray(),
                            CommandSchema.StubDefaultCommand);

                        _helpTextRenderer.RenderHelpText(_console, helpTextSource);
                    }

                    return isError ? -1 : 0;
                }

                // Show version if it was requested without specifying a command
                if (commandInput.IsVersionRequested() && !commandInput.IsCommandSpecified())
                {
                    _console.Output.WriteLine(_metadata.VersionText);

                    return 0;
                }

                // Show help if it was requested
                if (commandInput.IsHelpRequested())
                {
                    var helpTextSource = new HelpTextSource(_metadata, availableCommandSchemas, targetCommandSchema);
                    _helpTextRenderer.RenderHelpText(_console, helpTextSource);

                    return 0;
                }

                // Create an instance of the command
                var command = _commandFactory.CreateCommand(targetCommandSchema);

                // Populate command with options according to its schema
                _commandInitializer.InitializeCommand(command, targetCommandSchema, commandInput);

                // Execute command
                await command.ExecuteAsync(_console);

                return 0;
            }
            catch (Exception ex)
            {
                // We want to catch exceptions in order to print errors and return correct exit codes.
                // Also, by doing this we get rid of the annoying Windows troubleshooting dialog that shows up on unhandled exceptions.

                // In case we catch a CliFx-specific exception, we want to just show the error message, not the stack trace.
                // Stack trace isn't very useful to the user if the exception is not really coming from their code.

                // CommandException is the same, but it also lets users specify exit code so we want to return that instead of default.

                var message = ex is CliFxException && !ex.Message.IsNullOrWhiteSpace() ? ex.Message : ex.ToString();
                var exitCode = ex is CommandException commandEx ? commandEx.ExitCode : ex.HResult;

                _console.WithForegroundColor(ConsoleColor.Red, () => _console.Error.WriteLine(message));

                return exitCode;
            }
        }
    }
}