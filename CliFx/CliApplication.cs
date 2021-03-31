using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Exceptions;
using CliFx.Formatting;
using CliFx.Infrastructure;
using CliFx.Input;
using CliFx.Schema;
using CliFx.Suggestions;
using CliFx.Utils;
using CliFx.Utils.Extensions;

namespace CliFx
{
    /// <summary>
    /// Command line application facade.
    /// </summary>
    public class CliApplication
    {
        /// <summary>
        /// Application metadata.
        /// </summary>
        public ApplicationMetadata Metadata { get; }

        /// <summary>
        /// Application configuration.
        /// </summary>
        public ApplicationConfiguration Configuration { get; }

        private readonly IConsole _console;
        private readonly ITypeActivator _typeActivator;

        private readonly CommandBinder _commandBinder;
        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// Initializes an instance of <see cref="CliApplication"/>.
        /// </summary>
        public CliApplication(
            ApplicationMetadata metadata,
            ApplicationConfiguration configuration,
            IConsole console,
            ITypeActivator typeActivator,
            IFileSystem fileSystem)
        {
            Metadata = metadata;
            Configuration = configuration;
            _console = console;
            _typeActivator = typeActivator;

            _commandBinder = new CommandBinder(typeActivator);
            _fileSystem = fileSystem;
        }

        private bool IsDebugModeEnabled(CommandInput commandInput) =>
            Configuration.IsDebugModeAllowed && commandInput.IsDebugDirectiveSpecified;

        private bool IsPreviewModeEnabled(CommandInput commandInput) =>
            Configuration.IsPreviewModeAllowed && commandInput.IsPreviewDirectiveSpecified;

        private bool IsSuggestModeEnabled(CommandInput commandInput) =>
            Configuration.IsSuggestModeAllowed && commandInput.IsSuggestDirectiveSpecified;

        private bool ShouldShowHelpText(CommandSchema commandSchema, CommandInput commandInput) =>
            commandSchema.IsHelpOptionAvailable && commandInput.IsHelpOptionSpecified ||
            // Show help text also in case the fallback default command is
            // executed without any arguments.
            commandSchema == FallbackDefaultCommand.Schema &&
            string.IsNullOrWhiteSpace(commandInput.CommandName) &&
            !commandInput.Parameters.Any() &&
            !commandInput.Options.Any();

        private bool ShouldShowVersionText(CommandSchema commandSchema, CommandInput commandInput) =>
            commandSchema.IsVersionOptionAvailable && commandInput.IsVersionOptionSpecified;

        private async ValueTask PromptDebuggerAsync()
        {
            using (_console.WithForegroundColor(ConsoleColor.Green))
            {
                var processId = ProcessEx.GetCurrentProcessId();

                _console.Output.WriteLine(
                    $"Attach debugger to PID {processId} to continue."
                );
            }

            // Try to also launch debugger ourselves (only works if VS is installed)
            Debugger.Launch();

            while (!Debugger.IsAttached)
            {
                await Task.Delay(100);
            }
        }

        private async ValueTask<int> RunAsync(ApplicationSchema applicationSchema, CommandInput commandInput)
        {
            // Handle debug directive
            if (IsDebugModeEnabled(commandInput))
            {
                await PromptDebuggerAsync();
            }

            // Handle preview directive
            if (IsPreviewModeEnabled(commandInput))
            {
                _console.Output.WriteCommandInput(commandInput);
                return 0;
            }

            // Handle suggest directive
            if (Configuration.IsSuggestModeAllowed)
            {
                new SuggestShellHookInstaller(_fileSystem).Install(Metadata.Title);
            }
            if (IsSuggestModeEnabled(commandInput))
            {
                new SuggestionService(applicationSchema, _fileSystem, commandInput.EnvironmentVariables)
                           .GetSuggestions(commandInput).ToList()
                           .ForEach(p => _console.Output.WriteLine(p));
                return 0;
            }

            // Try to get the command schema that matches the input
            var commandSchema =
                applicationSchema.TryFindCommand(commandInput.CommandName) ??
                applicationSchema.TryFindDefaultCommand() ??
                FallbackDefaultCommand.Schema;

            // Activate command instance
            var commandInstance = commandSchema == FallbackDefaultCommand.Schema
                ? new FallbackDefaultCommand() // bypass activator
                : (ICommand) _typeActivator.CreateInstance(commandSchema.Type);

            // Assemble help context
            var helpContext = new HelpContext(
                Metadata,
                applicationSchema,
                commandSchema,
                commandSchema.GetValues(commandInstance)
            );

            // Handle help option
            if (ShouldShowHelpText(commandSchema, commandInput))
            {
                _console.Output.WriteHelpText(helpContext);
                return 0;
            }

            // Handle version option
            if (ShouldShowVersionText(commandSchema, commandInput))
            {
                _console.Output.WriteLine(Metadata.Version);
                return 0;
            }

            // Starting from this point, we may produce exceptions that are meant for the
            // end user of the application (i.e. invalid input, command exception, etc).
            // Catch these exceptions here, print them to the console, and don't let them
            // propagate further.
            try
            {
                // Bind and execute command
                _commandBinder.Bind(commandInput, commandSchema, commandInstance);
                await commandInstance.ExecuteAsync(_console);

                return 0;
            }
            catch (CliFxException ex)
            {
                _console.Error.WriteException(ex);

                if (ex.ShowHelp)
                {
                    _console.Output.WriteLine();
                    _console.Output.WriteHelpText(helpContext);
                }

                return ex.ExitCode;
            }
        }

        /// <summary>
        /// Runs the application with the specified command line arguments and environment variables.
        /// Returns an exit code which indicates whether the application completed successfully.
        /// </summary>
        /// <remarks>
        /// When running WITHOUT debugger (i.e. in production), this method swallows all exceptions and
        /// reports them to the console.
        /// </remarks>
        public async ValueTask<int> RunAsync(
            IReadOnlyList<string> commandLineArguments,
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            try
            {
                // Console colors may have already been overriden by the parent process,
                // so we need to reset it to make sure that everything we write looks properly.
                _console.ResetColor();

                var applicationSchema = ApplicationSchema.Resolve(Configuration.CommandTypes);

                var commandInput = CommandInput.Parse(
                    commandLineArguments,
                    environmentVariables,
                    applicationSchema.GetCommandNames()
                );

                return await RunAsync(applicationSchema, commandInput);
            }
            // To prevent the app from showing the annoying troubleshooting dialog on Windows,
            // we handle all exceptions ourselves and print them to the console.
            //
            // We only want to do that if the app is running in production, which we infer
            // based on whether a debugger is attached to the process.
            //
            // When not running in production, we want the IDE to show exceptions to the
            // developer, so we don't swallow them in that case.
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                _console.Error.WriteException(ex);
                return 1;
            }
        }

        /// <summary>
        /// Runs the application with the specified command line arguments.
        /// Environment variables are resolved automatically.
        /// Returns an exit code which indicates whether the application completed successfully.
        /// </summary>
        /// <remarks>
        /// When running WITHOUT debugger (i.e. in production), this method swallows all exceptions and
        /// reports them to the console.
        /// </remarks>
        public async ValueTask<int> RunAsync(IReadOnlyList<string> commandLineArguments) => await RunAsync(
            commandLineArguments,
            // Use case-sensitive comparison because environment variables are
            // case-sensitive on Linux and macOS (but not on Windows).
            Environment
                .GetEnvironmentVariables()
                .ToDictionary<string, string>(StringComparer.Ordinal)
        );

        /// <summary>
        /// Runs the application.
        /// Command line arguments and environment variables are resolved automatically.
        /// Returns an exit code which indicates whether the application completed successfully.
        /// </summary>
        /// <remarks>
        /// When running WITHOUT debugger (i.e. in production), this method swallows all exceptions and
        /// reports them to the console.
        /// </remarks>
        public async ValueTask<int> RunAsync() => await RunAsync(
            Environment.GetCommandLineArgs()
                .Skip(1) // first element is the file path
                .ToArray()
        );
    }
}
