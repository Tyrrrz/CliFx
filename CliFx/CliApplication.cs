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

        private async ValueTask<int?> HandleDebugDirectiveAsync(CommandLineInput commandInput)
        {
            // Debug mode is enabled if it's allowed in the application and it was requested via corresponding directive
            var isDebugMode = _configuration.IsDebugModeAllowed && commandInput.IsDebugDirectiveSpecified;

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

        private int? HandlePreviewDirective(CommandLineInput commandInput)
        {
            // Preview mode is enabled if it's allowed in the application and it was requested via corresponding directive
            var isPreviewMode = _configuration.IsPreviewModeAllowed && commandInput.IsPreviewDirectiveSpecified;

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

        private int? HandleVersionOption(CommandLineInput commandInput)
        {
            // Version should be rendered if it was requested on a default command
            var shouldRenderVersion = !commandInput.Arguments.Any() && commandInput.IsVersionOptionSpecified;

            // If shouldn't render version, pass execution to the next handler
            if (!shouldRenderVersion)
                return null;

            // Render version text
            _console.Output.WriteLine(_metadata.VersionText);

            // Short-circuit with exit code 0
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

                var command = applicationSchema.TryInitializeCommand(commandLineInput, environmentVariables, _typeActivator);

                await command.ExecuteAsync(_console);

                return 0;
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