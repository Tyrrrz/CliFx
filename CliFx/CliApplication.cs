using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private async Task<int?> HandleDebugDirectiveAsync(CommandInput commandInput)
        {
            // Debug mode is enabled if it's allowed in the application and it was requested via corresponding directive
            var isDebugMode = _configuration.IsDebugModeAllowed && commandInput.IsDebugDirectiveSpecified();

            // If not in debug mode, pass execution to the next handler
            if (!isDebugMode)
                return null;

            // Inform user which process they need to attach debugger to
            _console.WithForegroundColor(ConsoleColor.Green,
                () => _console.Output.WriteLine($"Attach debugger to PID {Process.GetCurrentProcess().Id} to continue."));

            // Wait until debugger is attached
            while (!Debugger.IsAttached)
                await Task.Delay(100);

            // Debug directive never short-circuits
            return null;
        }

        private int? HandlePreviewDirective(CommandInput commandInput)
        {
            // Preview mode is enabled if it's allowed in the application and it was requested via corresponding directive
            var isPreviewMode = _configuration.IsPreviewModeAllowed && commandInput.IsPreviewDirectiveSpecified();

            // If not in preview mode, pass execution to the next handler
            if (!isPreviewMode)
                return null;

            // Render command name
            _console.Output.WriteLine($"Command name: {commandInput.CommandName}");
            _console.Output.WriteLine();

            // Render directives
            _console.Output.WriteLine("Directives:");
            foreach (var directive in commandInput.Directives)
            {
                _console.Output.Write(" ");
                _console.Output.WriteLine(directive);
            }

            // Margin
            _console.Output.WriteLine();

            // Render options
            _console.Output.WriteLine("Options:");
            foreach (var option in commandInput.Options)
            {
                _console.Output.Write(" ");
                _console.Output.WriteLine(option);
            }

            // Short-circuit with exit code 0
            return 0;
        }

        private int? HandleVersionOption(CommandInput commandInput)
        {
            // Version should be rendered if it was requested on a default command
            var shouldRenderVersion = !commandInput.IsCommandSpecified() && commandInput.IsVersionOptionSpecified();

            // If shouldn't render version, pass execution to the next handler
            if (!shouldRenderVersion)
                return null;

            // Render version text
            _console.Output.WriteLine(_metadata.VersionText);

            // Short-circuit with exit code 0
            return 0;
        }

        private int? HandleHelpOption(CommandInput commandInput,
            IReadOnlyList<CommandSchema> availableCommandSchemas, CommandSchema targetCommandSchema)
        {
            // Help should be rendered if it was requested, or when executing a command which isn't defined
            var shouldRenderHelp = commandInput.IsHelpOptionSpecified() || targetCommandSchema == null;

            // If shouldn't render help, pass execution to the next handler
            if (!shouldRenderHelp)
                return null;

            // Keep track whether there was an error in the input
            var isError = false;

            // If target command isn't defined, find its contextual replacement
            if (targetCommandSchema == null)
            {
                // If command was specified, inform the user that it's not defined
                if (commandInput.IsCommandSpecified())
                {
                    _console.WithForegroundColor(ConsoleColor.Red,
                        () => _console.Error.WriteLine($"Specified command [{commandInput.CommandName}] is not defined."));

                    isError = true;
                }

                // Replace target command with closest parent of specified command
                targetCommandSchema = availableCommandSchemas.FindParent(commandInput.CommandName);

                // If there's no parent, replace with stub default command
                if (targetCommandSchema == null)
                {
                    targetCommandSchema = CommandSchema.StubDefaultCommand;
                    availableCommandSchemas = availableCommandSchemas.Concat(CommandSchema.StubDefaultCommand).ToArray();
                }
            }

            // Build help text source
            var helpTextSource = new HelpTextSource(_metadata, availableCommandSchemas, targetCommandSchema);

            // Render help text
            _helpTextRenderer.RenderHelpText(_console, helpTextSource);

            // Short-circuit with appropriate exit code
            return isError ? -1 : 0;
        }

        private async Task<int> HandleCommandExecutionAsync(CommandInput commandInput, CommandSchema targetCommandSchema)
        {
            // Create an instance of the command
            var command = _commandFactory.CreateCommand(targetCommandSchema);

            // Populate command with options according to its schema
            _commandInitializer.InitializeCommand(command, targetCommandSchema, commandInput);

            // Execute command
            await command.ExecuteAsync(_console);

            // Finish the chain with exit code 0
            return 0;
        }

        /// <inheritdoc />
        public async Task<int> RunAsync(IReadOnlyList<string> commandLineArguments)
        {
            commandLineArguments.GuardNotNull(nameof(commandLineArguments));

            try
            {
                // Parse command input from arguments
                var commandInput = _commandInputParser.ParseCommandInput(commandLineArguments);

                // Get schemas for all available command types
                var availableCommandSchemas = _commandSchemaResolver.GetCommandSchemas(_configuration.CommandTypes);

                // Find command schema matching the name specified in the input
                var targetCommandSchema = availableCommandSchemas.FindByName(commandInput.CommandName);

                // Chain handlers until the first one that produces an exit code
                return
                    await HandleDebugDirectiveAsync(commandInput) ??
                    HandlePreviewDirective(commandInput) ??
                    HandleVersionOption(commandInput) ??
                    HandleHelpOption(commandInput, availableCommandSchemas, targetCommandSchema) ??
                    await HandleCommandExecutionAsync(commandInput, targetCommandSchema);
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