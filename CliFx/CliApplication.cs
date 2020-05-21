using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Domain;
using CliFx.Exceptions;
using CliFx.Internal;

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
            ApplicationMetadata metadata, ApplicationConfiguration configuration,
            IConsole console, ITypeActivator typeActivator)
        {
            _metadata = metadata;
            _configuration = configuration;
            _console = console;
            _typeActivator = typeActivator;

            _helpTextWriter = new HelpTextWriter(metadata, console, typeActivator);
        }

        private void WriteError(string message) => _console.WithForegroundColor(ConsoleColor.Red, () =>
            _console.Error.WriteLine(message));

        private async ValueTask WaitForDebuggerAsync()
        {
            var processId = ProcessEx.GetCurrentProcessId();

            _console.WithForegroundColor(ConsoleColor.Green, () =>
                _console.Output.WriteLine($"Attach debugger to PID {processId} to continue."));

            while (!Debugger.IsAttached)
                await Task.Delay(100);
        }

        private void WriteCommandLineInput(CommandLineInput input)
        {
            // Command name
            if (!string.IsNullOrWhiteSpace(input.CommandName))
            {
                _console.WithForegroundColor(ConsoleColor.Cyan, () =>
                    _console.Output.Write(input.CommandName));

                _console.Output.Write(' ');
            }

            // Parameters
            foreach (var parameter in input.Parameters)
            {
                _console.Output.Write('<');

                _console.WithForegroundColor(ConsoleColor.White, () =>
                    _console.Output.Write(parameter));

                _console.Output.Write('>');
                _console.Output.Write(' ');
            }

            // Options
            foreach (var (alias, values) in input.Options)
            {
                _console.Output.Write('[');

                _console.WithForegroundColor(ConsoleColor.White, () =>
                {
                    // Alias
                    _console.Output.Write(alias.PrefixDashes());

                    // Values
                    foreach (var value in values)
                    {
                        _console.Output.Write(' ');
                        _console.Output.Write('"');
                        _console.Output.Write(value);
                        _console.Output.Write('"');
                    }
                });

                _console.Output.Write(']');
                _console.Output.Write(' ');
            }

            _console.Output.WriteLine();
        }

        private bool IsDebugDirectiveSpecified(CommandLineInput input) =>
            _configuration.IsDebugModeAllowed &&
            input.Directives.Any(d => string.Equals(d, "debug", StringComparison.OrdinalIgnoreCase));

        private bool IsPreviewDirectiveSpecified(CommandLineInput input) =>
            _configuration.IsPreviewModeAllowed &&
            input.Directives.Any(d => string.Equals(d, "preview", StringComparison.OrdinalIgnoreCase));

        private ICommand GetCommandInstance(CommandSchema commandSchema) =>
            commandSchema != StubDefaultCommand.Schema
                ? (ICommand) _typeActivator.CreateInstance(commandSchema.Type)
                : new StubDefaultCommand();

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
                var applicationSchema = ApplicationSchema.Resolve(_configuration.CommandTypes);
                var input = CommandLineInput.Parse(commandLineArguments, applicationSchema.GetCommandNames());

                // Debug mode
                if (IsDebugDirectiveSpecified(input))
                {
                    // Ensure debugger is attached and continue
                    await WaitForDebuggerAsync();
                }

                // Preview mode
                if (IsPreviewDirectiveSpecified(input))
                {
                    WriteCommandLineInput(input);
                    return ExitCode.Success;
                }

                // Try to get the command matching the input or fallback to default
                var command =
                    applicationSchema.TryFindCommand(input.CommandName) ??
                    applicationSchema.TryFindDefaultCommand() ??
                    StubDefaultCommand.Schema;

                // Version option
                if (command.IsVersionOptionSpecified(input))
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
                if (command.IsHelpOptionSpecified(input))
                {
                    _helpTextWriter.Write(applicationSchema, command, defaultValues);
                    return ExitCode.Success;
                }

                // Bind actual values from arguments
                command.Bind(instance, input, environmentVariables);

                try
                {
                    // Execute the command
                    await instance.ExecuteAsync(_console);
                    return ExitCode.Success;
                }
                // This handles both domain exceptions from CliFx as well as exceptions
                // thrown in order to short-circuit command execution in case of an error.
                catch (CliFxException ex)
                {
                    // We want minimal output from expected exceptions
                    WriteError(ex.GetConciseMessage());

                    // The exception may trigger help text to provide additional info to the user
                    if (ex.ShowHelp)
                        _helpTextWriter.Write(applicationSchema, command, defaultValues);

                    return ex.ExitCode;
                }
            }
            // To prevent the app from showing the annoying Windows troubleshooting dialog,
            // we handle all exceptions and write them to the console nicely.
            // However, we don't want to swallow unhandled exceptions when the debugger is attached,
            // because we still want the IDE to show unhandled exceptions so that the dev can fix them.
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                WriteError(ex.ToString());
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
            var environmentVariables = Environment.GetEnvironmentVariables()
                .Cast<DictionaryEntry>()
                .ToDictionary(e => (string) e.Key, e => (string) e.Value, StringComparer.OrdinalIgnoreCase);

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
        private static class ExitCode
        {
            public const int Success = 0;

            public static int FromException(Exception ex) =>
                ex is CliFxException localEx
                    ? localEx.ExitCode
                    : ex.HResult;
        }

        [Command]
        private class StubDefaultCommand : ICommand
        {
            public static CommandSchema Schema { get; } = CommandSchema.TryResolve(typeof(StubDefaultCommand))!;

            public ValueTask ExecuteAsync(IConsole console) => throw new CliFxException(showHelp: true);
        }
    }
}