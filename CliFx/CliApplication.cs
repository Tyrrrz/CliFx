using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Models;
using CliFx.Services;

namespace CliFx
{
    public partial class CliApplication : ICliApplication
    {
        private readonly ApplicationMetadata _applicationMetadata;
        private readonly IReadOnlyList<Type> _commandTypes;

        private readonly IConsole _console;
        private readonly ICommandInputParser _commandInputParser;
        private readonly ICommandSchemaResolver _commandSchemaResolver;
        private readonly ICommandFactory _commandFactory;
        private readonly ICommandInitializer _commandInitializer;
        private readonly ICommandHelpTextRenderer _commandHelpTextRenderer;

        public CliApplication(ApplicationMetadata applicationMetadata, IReadOnlyList<Type> commandTypes,
            IConsole console, ICommandInputParser commandInputParser, ICommandSchemaResolver commandSchemaResolver,
            ICommandFactory commandFactory, ICommandInitializer commandInitializer, ICommandHelpTextRenderer commandHelpTextRenderer)
        {
            _applicationMetadata = applicationMetadata;
            _commandTypes = commandTypes;

            _console = console;
            _commandInputParser = commandInputParser;
            _commandSchemaResolver = commandSchemaResolver;
            _commandFactory = commandFactory;
            _commandInitializer = commandInitializer;
            _commandHelpTextRenderer = commandHelpTextRenderer;
        }

        public async Task<int> RunAsync(IReadOnlyList<string> commandLineArguments)
        {
            try
            {
                var commandInput = _commandInputParser.ParseInput(commandLineArguments);

                var availableCommandSchemas = _commandSchemaResolver.GetCommandSchemas(_commandTypes);
                var matchingCommandSchema = availableCommandSchemas.FindByName(commandInput.CommandName);

                // Fail if there are no commands defined
                if (!availableCommandSchemas.Any())
                {
                    _console.WithForegroundColor(ConsoleColor.Red,
                        () => _console.Error.WriteLine("There are no commands defined in this application."));

                    return -1;
                }

                // Handle cases where requested command is not defined
                if (matchingCommandSchema == null)
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

                    // Use a stub if parent command schema is not found
                    if (parentCommandSchema == null)
                    {
                        parentCommandSchema = _commandSchemaResolver.GetCommandSchema(typeof(StubDefaultCommand));
                        availableCommandSchemas = availableCommandSchemas.Concat(new[] { parentCommandSchema }).ToArray();
                    }

                    // Show help
                    _commandHelpTextRenderer.RenderHelpText(_applicationMetadata, availableCommandSchemas, parentCommandSchema);

                    return isError ? -1 : 0;
                }

                // Show version if it was requested without specifying a command
                if (commandInput.IsVersionRequested() && !commandInput.IsCommandSpecified())
                {
                    _console.Output.WriteLine(_applicationMetadata.VersionText);

                    return 0;
                }

                // Show help if it was requested
                if (commandInput.IsHelpRequested())
                {
                    _commandHelpTextRenderer.RenderHelpText(_applicationMetadata, availableCommandSchemas, matchingCommandSchema);

                    return 0;
                }

                // Create an instance of the command
                var command = _commandFactory.CreateCommand(matchingCommandSchema.Type);

                // Populate command with options according to its schema
                _commandInitializer.InitializeCommand(command, matchingCommandSchema, commandInput);

                await command.ExecuteAsync(_console);

                return 0;
            }
            catch (Exception ex)
            {
                var message = ex is CliFxException ? ex.Message : ex.ToString();
                var exitCode = ex is CommandErrorException errorException ? errorException.ExitCode : -1;

                _console.WithForegroundColor(ConsoleColor.Red, () => _console.Error.WriteLine(message));

                return exitCode;
            }
        }
    }

    public partial class CliApplication
    {
        [Command]
        private sealed class StubDefaultCommand : ICommand
        {
            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
        }
    }
}