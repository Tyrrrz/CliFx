﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CliFx.Exceptions;
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
            _metadata = metadata;
            _configuration = configuration;

            _console = console;
            _commandInputParser = commandInputParser;
            _commandSchemaResolver = commandSchemaResolver;
            _commandFactory = commandFactory;
            _commandInitializer = commandInitializer;
            _helpTextRenderer = helpTextRenderer;
        }

        private async ValueTask<int?> HandleDebugDirectiveAsync(CommandInput commandInput)
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
            _console.Output.WriteLine($"Arguments: {string.Join(" ", commandInput.Arguments)}");
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
            var shouldRenderVersion = !commandInput.HasArguments() && commandInput.IsVersionOptionSpecified();

            // If shouldn't render version, pass execution to the next handler
            if (!shouldRenderVersion)
                return null;

            // Render version text
            _console.Output.WriteLine(_metadata.VersionText);

            // Short-circuit with exit code 0
            return 0;
        }

        private int? HandleHelpOption(CommandInput commandInput,
            IReadOnlyList<CommandSchema> availableCommandSchemas, CommandCandidate? commandCandidate)
        {
            // Help should be rendered if it was requested, or when executing a command which isn't defined
            var shouldRenderHelp = commandInput.IsHelpOptionSpecified() || commandCandidate == null;

            // If shouldn't render help, pass execution to the next handler
            if (!shouldRenderHelp)
                return null;

            // Keep track whether there was an error in the input
            var isError = false;

            // Report error if no command matched the arguments
            if (commandCandidate is null)
            {
                // If a command was specified, inform the user that the command is not defined
                if (commandInput.HasArguments())
                {
                    _console.WithForegroundColor(ConsoleColor.Red,
                        () => _console.Error.WriteLine($"No command could be matched for input [{string.Join(" ", commandInput.Arguments)}]"));
                    isError = true;
                }

                commandCandidate = new CommandCandidate(CommandSchema.StubDefaultCommand, new string[0], commandInput);
            }

            // Build help text source
            var helpTextSource = new HelpTextSource(_metadata, availableCommandSchemas, commandCandidate.Schema);

            // Render help text
            _helpTextRenderer.RenderHelpText(_console, helpTextSource);

            // Short-circuit with appropriate exit code
            return isError ? -1 : 0;
        }

        private async ValueTask<int> HandleCommandExecutionAsync(CommandCandidate? commandCandidate)
        {
            if (commandCandidate is null)
            {
                throw new ArgumentException("Cannot execute command because it was not found.");
            }

            // Create an instance of the command
            var command = _commandFactory.CreateCommand(commandCandidate.Schema);

            // Populate command with options and arguments according to its schema
            _commandInitializer.InitializeCommand(command, commandCandidate);

            // Execute command
            await command.ExecuteAsync(_console);

            // Finish the chain with exit code 0
            return 0;
        }

        /// <inheritdoc />
        public async ValueTask<int> RunAsync(IReadOnlyList<string> commandLineArguments)
        {
            try
            {
                // Parse command input from arguments
                var commandInput = _commandInputParser.ParseCommandInput(commandLineArguments);

                // Get schemas for all available command types
                var availableCommandSchemas = _commandSchemaResolver.GetCommandSchemas(_configuration.CommandTypes);

                // Find command schema matching the name specified in the input
                var commandCandidate = _commandSchemaResolver.GetTargetCommandSchema(availableCommandSchemas, commandInput);

                // Chain handlers until the first one that produces an exit code
                return
                    await HandleDebugDirectiveAsync(commandInput) ??
                    HandlePreviewDirective(commandInput) ??
                    HandleVersionOption(commandInput) ??
                    HandleHelpOption(commandInput, availableCommandSchemas, commandCandidate) ??
                    await HandleCommandExecutionAsync(commandCandidate);
            }
            catch (Exception ex)
            {
                // We want to catch exceptions in order to print errors and return correct exit codes.
                // Doing this also gets rid of the annoying Windows troubleshooting dialog that shows up on unhandled exceptions.

                // Prefer showing message without stack trace on exceptions coming from CliFx or on CommandException
                if (!string.IsNullOrWhiteSpace(ex.Message) && (ex is CliFxException || ex is CommandException))
                {
                    _console.WithForegroundColor(ConsoleColor.Red, () => _console.Error.WriteLine(ex.Message));
                }
                else
                {
                    _console.WithForegroundColor(ConsoleColor.Red, () => _console.Error.WriteLine(ex));
                }

                // Return exit code if it was specified via CommandException
                if (ex is CommandException commandException)
                {
                    return commandException.ExitCode;
                }
                else
                {
                    return ex.HResult;
                }
            }
        }
    }
}