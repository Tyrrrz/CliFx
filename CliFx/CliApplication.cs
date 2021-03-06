using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using CliFx.Parsing;
using CliFx.Schema;
using CliFx.Utils;

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

        private readonly HelpTextWriter _helpTextWriter;

        /// <summary>
        /// Initializes an instance of <see cref="CliApplication"/>.
        /// </summary>
        public CliApplication(
            ApplicationMetadata metadata,
            ApplicationConfiguration configuration,
            IConsole console,
            ITypeActivator typeActivator)
        {
            _metadata = metadata;
            _configuration = configuration;
            _console = console;
            _typeActivator = typeActivator;

            _helpTextWriter = new HelpTextWriter(metadata, console);
        }

        private async ValueTask LaunchAndWaitForDebuggerAsync()
        {
            var processId = ProcessEx.GetCurrentProcessId();

            using (_console.WithForegroundColor(ConsoleColor.Green))
                _console.Output.WriteLine($"Attach debugger to PID {processId} to continue.");

            Debugger.Launch();

            while (!Debugger.IsAttached)
            {
                await Task.Delay(100);
            }
        }

        private void WriteCommandLineInput(CommandInput input)
        {
            // Command name
            if (!string.IsNullOrWhiteSpace(input.CommandName))
            {
                using (_console.WithForegroundColor(ConsoleColor.Cyan))
                    _console.Output.Write(input.CommandName);

                _console.Output.Write(' ');
            }

            // Parameters
            foreach (var parameter in input.Parameters)
            {
                _console.Output.Write('<');

                using (_console.WithForegroundColor(ConsoleColor.White))
                    _console.Output.Write(parameter);

                _console.Output.Write('>');
                _console.Output.Write(' ');
            }

            // Options
            foreach (var option in input.Options)
            {
                _console.Output.Write('[');

                using (_console.WithForegroundColor(ConsoleColor.White))
                {
                    // Alias
                    _console.Output.Write(option.GetRawAlias());

                    // Values
                    if (option.Values.Any())
                    {
                        _console.Output.Write(' ');
                        _console.Output.Write(option.GetRawValues());
                    }
                }

                _console.Output.Write(']');
                _console.Output.Write(' ');
            }

            _console.Output.WriteLine();
        }

        private ICommand GetCommandInstance(CommandSchema command) =>
            command != FallbackDefaultCommand.Schema
                ? (ICommand) _typeActivator.CreateInstance(command.Type)
                : new FallbackDefaultCommand();

        /// <summary>
        /// Runs the application with specified command line arguments and environment variables, and returns the exit code.
        /// </summary>
        /// <remarks>
        /// If a <see cref="CommandException"/> is thrown during command execution, it will be handled and routed to the console.
        /// Additionally, if the debugger is not attached (i.e. the app is running in production), all other exceptions thrown within
        /// this method will be handled and routed to the console as well.
        /// </remarks>
        public async ValueTask<int> RunAsync(
            IReadOnlyList<string> commandLineArguments,
            IReadOnlyDictionary<string, string> environmentVariables)
        {
            try
            {
                var root = ApplicationSchema.Resolve(_configuration.CommandTypes);
                var input = CommandInput.Parse(commandLineArguments, root.GetCommandNames());

                // Debug mode
                if (_configuration.IsDebugModeAllowed && input.IsDebugDirectiveSpecified)
                {
                    await LaunchAndWaitForDebuggerAsync();
                }

                // Preview mode
                if (_configuration.IsPreviewModeAllowed && input.IsPreviewDirectiveSpecified)
                {
                    WriteCommandLineInput(input);
                    return ExitCode.Success;
                }

                // Try to get the command matching the input or fallback to default
                var command =
                    root.TryFindCommand(input.CommandName) ??
                    root.TryFindDefaultCommand() ??
                    FallbackDefaultCommand.Schema;

                // Version option
                if (command.IsVersionOptionAvailable && input.IsVersionOptionSpecified)
                {
                    _console.Output.WriteLine(_metadata.VersionText);
                    return ExitCode.Success;
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
                    _helpTextWriter.Write(root, command, defaultValues);
                    return ExitCode.Success;
                }

                // Bind arguments
                try
                {
                    command.Bind(instance, input, environmentVariables);
                }
                // This may throw exceptions which are useful only to the end-user
                catch (CliFxException ex)
                {
                    using (_console.WithBackgroundColor(ConsoleColor.Red))
                        _console.Error.WriteLine(ex.ToString());

                    _helpTextWriter.Write(root, command, defaultValues);

                    return ExitCode.FromException(ex);
                }

                // Execute the command
                try
                {
                    await instance.ExecuteAsync(_console);
                    return ExitCode.Success;
                }
                // Swallow command exceptions and route them to the console
                catch (CommandException ex)
                {
                    using (_console.WithForegroundColor(ConsoleColor.Red))
                        _console.Error.WriteLine(ex.ToString());

                    if (ex.ShowHelp)
                    {
                        _helpTextWriter.Write(root, command, defaultValues);
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
                using (_console.WithColors(ConsoleColor.White, ConsoleColor.DarkRed))
                    _console.Error.Write("ERROR:");

                _console.Error.Write(" ");
                _console.WriteException(ex);

                return ExitCode.FromException(ex);
            }
        }

        /// <summary>
        /// Runs the application with specified command line arguments and returns the exit code.
        /// Environment variables are retrieved automatically.
        /// </summary>
        /// <remarks>
        /// If a <see cref="CommandException"/> is thrown during command execution, it will be handled and routed to the console.
        /// Additionally, if the debugger is not attached (i.e. the app is running in production), all other exceptions thrown within
        /// this method will be handled and routed to the console as well.
        /// </remarks>
        public async ValueTask<int> RunAsync(IReadOnlyList<string> commandLineArguments)
        {
            // Environment variable names are case-insensitive on Windows but are case-sensitive on Linux and macOS
            var environmentVariables = Environment.GetEnvironmentVariables()
                .Cast<DictionaryEntry>()
                .ToDictionary(e => (string) e.Key, e => (string) e.Value, StringComparer.Ordinal);

            return await RunAsync(commandLineArguments, environmentVariables);
        }

        /// <summary>
        /// Runs the application and returns the exit code.
        /// Command line arguments and environment variables are retrieved automatically.
        /// </summary>
        /// <remarks>
        /// If a <see cref="CommandException"/> is thrown during command execution, it will be handled and routed to the console.
        /// Additionally, if the debugger is not attached (i.e. the app is running in production), all other exceptions thrown within
        /// this method will be handled and routed to the console as well.
        /// </remarks>
        public async ValueTask<int> RunAsync()
        {
            var commandLineArguments = Environment.GetCommandLineArgs()
                .Skip(1)
                .ToArray();

            return await RunAsync(commandLineArguments);
        }
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
