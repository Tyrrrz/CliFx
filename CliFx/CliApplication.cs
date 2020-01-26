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
    public partial class CliApplication
    {
        private readonly ApplicationMetadata _metadata;
        private readonly ApplicationConfiguration _configuration;
        private readonly IConsole _console;
        private readonly ITypeActivator _typeActivator;

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
        }

        private async ValueTask<int?> HandleDebugDirectiveAsync(CommandLineInput commandLineInput)
        {
            var isDebugMode = _configuration.IsDebugModeAllowed && commandLineInput.IsDebugDirectiveSpecified;
            if (!isDebugMode)
                return null;

            _console.WithForegroundColor(ConsoleColor.Green,
                () => _console.Output.WriteLine($"Attach debugger to PID {Process.GetCurrentProcess().Id} to continue."));

            while (!Debugger.IsAttached)
                await Task.Delay(100);

            return null;
        }

        private int? HandlePreviewDirective(CommandLineInput commandLineInput)
        {
            var isPreviewMode = _configuration.IsPreviewModeAllowed && commandLineInput.IsPreviewDirectiveSpecified;
            if (!isPreviewMode)
                return null;

            // Render command name
            _console.Output.WriteLine($"Arguments: {string.Join(" ", commandLineInput.Arguments)}");
            _console.Output.WriteLine();

            // Render directives
            _console.Output.WriteLine("Directives:");
            foreach (var directive in commandLineInput.Directives)
            {
                _console.Output.Write(" ");
                _console.Output.WriteLine(directive);
            }

            // Margin
            _console.Output.WriteLine();

            // Render options
            _console.Output.WriteLine("Options:");
            foreach (var option in commandLineInput.Options)
            {
                _console.Output.Write(" ");
                _console.Output.WriteLine(option);
            }

            return 0;
        }

        private int? HandleVersionOption(CommandLineInput commandLineInput)
        {
            var shouldRenderVersion = !commandLineInput.Arguments.Any() && commandLineInput.IsVersionOptionSpecified;
            if (!shouldRenderVersion)
                return null;

            _console.Output.WriteLine(_metadata.VersionText);

            return 0;
        }

        private int? HandleHelpOption(
            ApplicationSchema applicationSchema,
            CommandLineInput commandLineInput)
        {
            var shouldRenderHelp =
                commandLineInput.IsHelpOptionSpecified ||
                !applicationSchema.Commands.Any(c => c.IsDefault) && !commandLineInput.Arguments.Any() && !commandLineInput.Options.Any();

            if (!shouldRenderHelp)
                return null;

            var commandSchema =
                applicationSchema.TryFindCommandSchema(commandLineInput) ??
                new CommandSchema(null!, null, null, new CommandParameterSchema[0], new CommandOptionSchema[0]);

            RenderHelp(applicationSchema, commandSchema);

            return 0;
        }

        private async ValueTask<int> HandleCommandExecutionAsync(
            ApplicationSchema applicationSchema,
            CommandLineInput commandLineInput,
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            var command = applicationSchema.InitializeCommand(commandLineInput, environmentVariables, _typeActivator);
            await command.ExecuteAsync(_console);

            return 0;
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
                    HandlePreviewDirective(commandLineInput) ??
                    HandleVersionOption(commandLineInput) ??
                    HandleHelpOption(applicationSchema, commandLineInput) ??
                    await HandleCommandExecutionAsync(applicationSchema, commandLineInput, environmentVariables);
            }
            catch (Exception ex)
            {
                // We want to catch exceptions in order to print errors and return correct exit codes.
                // Doing this also gets rid of the annoying Windows troubleshooting dialog that shows up on unhandled exceptions.

                // Prefer showing message without stack trace on exceptions coming from CliFx or on CommandException
                var errorMessage = !string.IsNullOrWhiteSpace(ex.Message) && (ex is CliFxException || ex is CommandException)
                    ? ex.Message
                    : ex.ToString();

                _console.WithForegroundColor(ConsoleColor.Red, () => _console.Error.WriteLine(errorMessage));

                return ex is CommandException commandException
                    ? commandException.ExitCode
                    : ex.HResult;
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