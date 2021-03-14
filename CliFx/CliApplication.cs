using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using CliFx.Input;
using CliFx.Schema;
using CliFx.Utils;
using CliFx.Utils.Extensions;

namespace CliFx
{
    /// <summary>
    /// Command line application facade.
    /// </summary>
    public partial class CliApplication
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

        /// <summary>
        /// Initializes an instance of <see cref="CliApplication"/>.
        /// </summary>
        public CliApplication(
            ApplicationMetadata metadata,
            ApplicationConfiguration configuration,
            IConsole console,
            ITypeActivator typeActivator)
        {
            Metadata = metadata;
            Configuration = configuration;
            _console = console;
            _typeActivator = typeActivator;
        }

        private async ValueTask LaunchAndWaitForDebuggerAsync()
        {
            var processId = ProcessEx.GetCurrentProcessId();

            using (_console.WithForegroundColor(ConsoleColor.Green))
            {
                _console.Output.WriteLine(
                    $"Attach debugger to PID {processId} to continue."
                );
            }

            Debugger.Launch();

            while (!Debugger.IsAttached)
            {
                await Task.Delay(100);
            }
        }

        private ICommand GetCommandInstance(CommandSchema command) =>
            command != FallbackDefaultCommand.Schema
                ? (ICommand) _typeActivator.CreateInstance(command.Type)
                : new FallbackDefaultCommand();

        /// <summary>
        /// Runs the application with the specified command line arguments and environment variables.
        /// Returns an exit code which indicates whether the application completed successfully.
        /// </summary>
        /// <remarks>
        /// When running WITHOUT debugger (i.e. in production), this method swallows all exceptions and
        /// reports them to the console.
        /// When running WITH debugger (i.e. while developing), this method only swallows
        /// <see cref="CommandException"/>.
        /// </remarks>
        public async ValueTask<int> RunAsync(
            IReadOnlyList<string> commandLineArguments,
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            try
            {
                // Console colors may have already been overriden by the parent process,
                // so we need to reset it to make sure that everything we write looks nice.
                _console.ResetColor();

                var root = ApplicationSchema.Resolve(Configuration.CommandTypes);
                var input = CommandInput.Parse(commandLineArguments, root.GetCommandNames());

                // Debug mode
                if (Configuration.IsDebugModeAllowed && input.IsDebugDirectiveSpecified)
                {
                    await LaunchAndWaitForDebuggerAsync();
                }

                // Preview mode
                if (Configuration.IsPreviewModeAllowed && input.IsPreviewDirectiveSpecified)
                {
                    _console.WriteCommandInput(input);
                    return 0;
                }

                // Try to get the command matching the input or fallback to default
                var command =
                    root.TryFindCommand(input.CommandName) ??
                    root.TryFindDefaultCommand() ??
                    FallbackDefaultCommand.Schema;

                // Version option
                if (command.IsVersionOptionAvailable && input.IsVersionOptionSpecified)
                {
                    _console.Output.WriteLine(Metadata.Version);
                    return 0;
                }

                // Get command instance (also used in help text)
                var instance = GetCommandInstance(command);

                // To avoid instantiating the command twice, we need to get default values
                // before the arguments are bound to the properties
                var defaultValues = command.GetArgumentValues(instance);

                // Help option
                if (command.IsHelpOptionAvailable && input.IsHelpOptionSpecified ||
                    command == FallbackDefaultCommand.Schema && !input.Parameters.Any() && !input.Options.Any())
                {
                    _console.WriteHelpText(Metadata, root, command, defaultValues);
                    return 0;
                }

                try
                {
                    command.Bind(instance, input, environmentVariables);

                    await instance.ExecuteAsync(_console);

                    return 0;
                }
                // Swallow command exceptions and route them to the console
                catch (CliFxException ex)
                {
                    // If the message is set, don't print the whole stack trace
                    if (!string.IsNullOrWhiteSpace(ex.ActualMessage))
                    {
                        using (_console.WithForegroundColor(ConsoleColor.Red))
                            _console.Error.WriteLine(ex.ActualMessage);
                    }
                    else
                    {
                        _console.WriteException(ex);
                    }

                    if (ex.ShowHelp)
                    {
                        _console.WriteHelpText(Metadata, root, command, defaultValues);
                    }

                    return ex.ExitCode;
                }
            }
            // To prevent the app from showing the annoying Windows troubleshooting dialog,
            // we handle all exceptions and route them to the console nicely.
            // However, we don't want to swallow unhandled exceptions when the debugger is attached,
            // because we still want the IDE to show them to the developer.
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                _console.WriteException(ex);
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
        /// When running WITH debugger (i.e. while developing), this method only swallows
        /// <see cref="CommandException"/>.
        /// </remarks>
        public async ValueTask<int> RunAsync(IReadOnlyList<string> commandLineArguments) => await RunAsync(
            commandLineArguments,
            // Use case-sensitive comparison because environment variables are
            // case-sensitive on Linux and macOS.
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
        /// When running WITH debugger (i.e. while developing), this method only swallows
        /// <see cref="CommandException"/>.
        /// </remarks>
        public async ValueTask<int> RunAsync() => await RunAsync(
            Environment.GetCommandLineArgs()
                .Skip(1) // first element is the file path
                .ToArray()
        );
    }

    public partial class CliApplication
    {
        // Fallback default command used when none is defined in the application
        [Command]
        private class FallbackDefaultCommand : ICommand
        {
            public static CommandSchema Schema { get; } = CommandSchema.Resolve(typeof(FallbackDefaultCommand));

            // Never actually executed
            [ExcludeFromCodeCoverage]
            public ValueTask ExecuteAsync(IConsole console) => default;
        }
    }
}
