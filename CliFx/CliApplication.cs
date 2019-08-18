using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Internal;
using CliFx.Models;
using CliFx.Services;

namespace CliFx
{
    /// <summary>
    /// Default implementation of <see cref="ICliApplication"/>.
    /// </summary>
    public partial class CliApplication : ICliApplication
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

        private IReadOnlyList<string> GetAvailableCommandSchemasValidationErrors(IReadOnlyList<CommandSchema> availableCommandSchemas)
        {
            var result = new List<string>();

            // Fail if there are no commands defined
            if (!availableCommandSchemas.Any())
            {
                result.Add("There are no commands defined.");
            }

            // Fail if there are commands that don't implement ICommand
            var nonImplementedCommandNames = availableCommandSchemas
                .Where(c => !c.Type.Implements(typeof(ICommand)))
                .Select(c => c.Name)
                .Distinct()
                .ToArray();

            foreach (var commandName in nonImplementedCommandNames)
            {
                result.Add(!commandName.IsNullOrWhiteSpace()
                    ? $"Command [{commandName}] doesn't implement ICommand."
                    : "Default command doesn't implement ICommand.");
            }

            // Fail if there are multiple commands with the same name
            var nonUniqueCommandNames = availableCommandSchemas
                .Select(c => c.Name)
                .GroupBy(i => i, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() >= 2)
                .SelectMany(g => g)
                .Distinct()
                .ToArray();

            foreach (var commandName in nonUniqueCommandNames)
            {
                result.Add(!commandName.IsNullOrWhiteSpace()
                    ? $"There are multiple commands defined with name [{commandName}]."
                    : "There are multiple default commands defined.");
            }

            // Fail if there are multiple options with the same name inside the same command
            foreach (var commandSchema in availableCommandSchemas)
            {
                var nonUniqueOptionNames = commandSchema.Options
                    .Where(o => !o.Name.IsNullOrWhiteSpace())
                    .Select(o => o.Name)
                    .GroupBy(i => i, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() >= 2)
                    .SelectMany(g => g)
                    .Distinct()
                    .ToArray();

                foreach (var optionName in nonUniqueOptionNames)
                {
                    result.Add(!commandSchema.Name.IsNullOrWhiteSpace()
                        ? $"There are multiple options defined with name [{optionName}] on command [{commandSchema.Name}]."
                        : $"There are multiple options defined with name [{optionName}] on default command.");
                }

                var nonUniqueOptionShortNames = commandSchema.Options
                    .Where(o => o.ShortName != null)
                    .Select(o => o.ShortName.Value)
                    .GroupBy(i => i)
                    .Where(g => g.Count() >= 2)
                    .SelectMany(g => g)
                    .Distinct()
                    .ToArray();

                foreach (var optionShortName in nonUniqueOptionShortNames)
                {
                    result.Add(!commandSchema.Name.IsNullOrWhiteSpace()
                        ? $"There are multiple options defined with short name [{optionShortName}] on command [{commandSchema.Name}]."
                        : $"There are multiple options defined with short name [{optionShortName}] on default command.");
                }
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<int> RunAsync(IReadOnlyList<string> commandLineArguments)
        {
            commandLineArguments.GuardNotNull(nameof(commandLineArguments));

            try
            {
                var commandInput = _commandInputParser.ParseInput(commandLineArguments);

                var availableCommandSchemas = _commandSchemaResolver.GetCommandSchemas(_configuration.CommandTypes);
                var matchingCommandSchema = availableCommandSchemas.FindByName(commandInput.CommandName);

                // Validate available command schemas
                var validationErrors = GetAvailableCommandSchemasValidationErrors(availableCommandSchemas);
                if (validationErrors.Any())
                {
                    foreach (var error in validationErrors)
                        _console.WithForegroundColor(ConsoleColor.Red, () => _console.Output.WriteLine(error));

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
                    var helpTextSource = new HelpTextSource(_metadata, availableCommandSchemas, parentCommandSchema);
                    _helpTextRenderer.RenderHelpText(_console, helpTextSource);

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
                    var helpTextSource = new HelpTextSource(_metadata, availableCommandSchemas, matchingCommandSchema);
                    _helpTextRenderer.RenderHelpText(_console, helpTextSource);

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

    public partial class CliApplication
    {
        [Command]
        private sealed class StubDefaultCommand : ICommand
        {
            public Task ExecuteAsync(IConsole console) => Task.CompletedTask;
        }
    }
}