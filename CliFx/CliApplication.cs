using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Domain;
using CliFx.Exceptions;

namespace CliFx
{
    /// <summary>
    /// Command line application facade.
    /// </summary>
    public class CliApplication
    {
        private readonly ApplicationMetadata _metadata;
        private readonly ApplicationConfiguration _configuration;
        private readonly IConsole _console;
        private readonly ITypeActivator _typeActivator;

        private readonly HelpTextWriter _helpTextWriter;

        /// <summary>
        /// Initializes an instance of <see cref="CliApplication"/>.
        /// </summary>
        public CliApplication(
            ApplicationMetadata metadata, ApplicationConfiguration configuration,
            IConsole console, ITypeActivator typeActivator)
        {
            _metadata = metadata;
            _configuration = configuration;
            _console = console;
            _typeActivator = typeActivator;

            _helpTextWriter = new HelpTextWriter(metadata, console);
        }

        private async ValueTask<int?> HandleDebugDirectiveAsync(CommandLineInput commandLineInput)
        {
            var isDebugMode = _configuration.IsDebugModeAllowed && commandLineInput.IsDebugDirectiveSpecified;
            if (!isDebugMode)
                return null;

            _console.WithForegroundColor(ConsoleColor.Green, () =>
                _console.Output.WriteLine($"Attach debugger to PID {Process.GetCurrentProcess().Id} to continue."));

            while (!Debugger.IsAttached)
                await Task.Delay(100);

            return null;
        }

        private int? HandlePreviewDirective(ApplicationSchema applicationSchema, CommandLineInput commandLineInput)
        {
            var isPreviewMode = _configuration.IsPreviewModeAllowed && commandLineInput.IsPreviewDirectiveSpecified;
            if (!isPreviewMode)
                return null;

            var commandSchema = applicationSchema.TryFindCommand(commandLineInput, out var argumentOffset);

            _console.Output.WriteLine("Parser preview:");

            // Command name
            if (commandSchema != null && argumentOffset > 0)
            {
                _console.WithForegroundColor(ConsoleColor.Cyan, () =>
                    _console.Output.Write(commandSchema.Name));

                _console.Output.Write(' ');
            }

            // Parameters
            foreach (var parameter in commandLineInput.UnboundArguments.Skip(argumentOffset))
            {
                _console.Output.Write('<');

                _console.WithForegroundColor(ConsoleColor.White, () =>
                    _console.Output.Write(parameter));

                _console.Output.Write('>');
                _console.Output.Write(' ');
            }

            // Options
            foreach (var option in commandLineInput.Options)
            {
                _console.Output.Write('[');

                _console.WithForegroundColor(ConsoleColor.White, () =>
                    _console.Output.Write(option));

                _console.Output.Write(']');
                _console.Output.Write(' ');
            }

            _console.Output.WriteLine();

            return 0;
        }

        private int? HandleVersionOption(CommandLineInput commandLineInput)
        {
            // Version option is available only on the default command (i.e. when arguments are not specified)
            var shouldRenderVersion = !commandLineInput.UnboundArguments.Any() && commandLineInput.IsVersionOptionSpecified;
            if (!shouldRenderVersion)
                return null;

            _console.Output.WriteLine(_metadata.VersionText);

            return 0;
        }

        private int? HandleHelpOption(ApplicationSchema applicationSchema, CommandLineInput commandLineInput)
        {
            // Help is rendered either when it's requested or when the user provides no arguments and there is no default command
            var shouldRenderHelp =
                commandLineInput.IsHelpOptionSpecified ||
                !applicationSchema.Commands.Any(c => c.IsDefault) && !commandLineInput.UnboundArguments.Any() && !commandLineInput.Options.Any();

            if (!shouldRenderHelp)
                return null;

            // Get the command schema that matches the input or use a dummy default command as a fallback
            var commandSchema =
                applicationSchema.TryFindCommand(commandLineInput) ??
                CommandSchema.StubDefaultCommand;

            _helpTextWriter.Write(applicationSchema, commandSchema);

            return 0;
        }

        private async ValueTask<int> HandleCommandExecutionAsync(
            ApplicationSchema applicationSchema,
            CommandLineInput commandLineInput,
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            await applicationSchema
                .InitializeEntryPoint(commandLineInput, environmentVariables, _typeActivator)
                .ExecuteAsync(_console);

            return 0;
        }

        /// <summary>
        /// Handle <see cref="CommandException"/>s differently from the rest because we want to
        /// display it different based on whether we are showing the help text or not.
        /// </summary>
        private int HandleCliFxException(IReadOnlyList<string> commandLineArguments, CliFxException cfe)
        {
            var showHelp = cfe.ShowHelp;

            var errorMessage = cfe.HasMessage
                ? cfe.Message
                : cfe.ToString();
               
            _console.WithForegroundColor(ConsoleColor.Red, () => _console.Error.WriteLine(errorMessage));            

            if (showHelp)
            {
                var applicationSchema = ApplicationSchema.Resolve(_configuration.CommandTypes);
                var commandLineInput = CommandLineInput.Parse(commandLineArguments);
                var commandSchema = applicationSchema.TryFindCommand(commandLineInput) ??
                    CommandSchema.StubDefaultCommand;
                _helpTextWriter.Write(applicationSchema, commandSchema);
            }

            return cfe.ExitCode;
        }

        /// <summary>
        /// Runs the application with specified command line arguments and environment variables, and returns the exit code.
        /// </summary>
        public async ValueTask<int> RunAsync(
            IReadOnlyList<string> commandLineArguments,
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            try
            {
                var applicationSchema = ApplicationSchema.Resolve(_configuration.CommandTypes);
                var commandLineInput = CommandLineInput.Parse(commandLineArguments);

                return
                    await HandleDebugDirectiveAsync(commandLineInput) ??
                    HandlePreviewDirective(applicationSchema, commandLineInput) ??
                    HandleVersionOption(commandLineInput) ??
                    HandleHelpOption(applicationSchema, commandLineInput) ??
                    await HandleCommandExecutionAsync(applicationSchema, commandLineInput, environmentVariables);
            }
            catch (CliFxException cfe)
            {
                // We want to catch exceptions in order to print errors and return correct exit codes.
                // Doing this also gets rid of the annoying Windows troubleshooting dialog that shows up on unhandled exceptions.
                var exitCode = HandleCliFxException(commandLineArguments, cfe);
                return exitCode;
            }
            catch (Exception ex)
            {
                // For all other errors, we just write the entire thing to stderr.
                _console.WithForegroundColor(ConsoleColor.Red, () => _console.Error.WriteLine(ex.ToString()));
                return ex.HResult;
            }
        }

        /// <summary>
        /// Runs the application with specified command line arguments and returns the exit code.
        /// Environment variables are retrieved automatically.
        /// </summary>
        public async ValueTask<int> RunAsync(IReadOnlyList<string> commandLineArguments)
        {
            var environmentVariables = Environment.GetEnvironmentVariables()
                .Cast<DictionaryEntry>()
                .ToDictionary(e => (string) e.Key, e => (string) e.Value, StringComparer.OrdinalIgnoreCase);

            return await RunAsync(commandLineArguments, environmentVariables);
        }

        /// <summary>
        /// Runs the application and returns the exit code.
        /// Command line arguments and environment variables are retrieved automatically.
        /// </summary>
        public async ValueTask<int> RunAsync()
        {
            var commandLineArguments = Environment.GetCommandLineArgs()
                .Skip(1)
                .ToArray();

            return await RunAsync(commandLineArguments);
        }
    }
}