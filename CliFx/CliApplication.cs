using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
                    _console.Output.WriteLine(alias);

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

        private async ValueTask<bool> HandleDebugDirectiveAsync(CommandLineInput input)
        {
            if (_configuration.IsDebugModeAllowed &&
                input.Directives.Any(d => string.Equals(d, "debug", StringComparison.OrdinalIgnoreCase)))
            {
                await WaitForDebuggerAsync();
                return true;
            }

            return false;
        }

        private bool HandlePreviewDirective(CommandLineInput input)
        {
            if (_configuration.IsPreviewModeAllowed &&
                input.Directives.Any(d => string.Equals(d, "preview", StringComparison.OrdinalIgnoreCase)))
            {
                WriteCommandLineInput(input);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Runs the application with specified command line arguments and environment variables, and returns the exit code.
        /// </summary>
        /// <remarks>
        /// This method swallows all exceptions and routes them to the console, but only if the debugger is not attached.
        /// If the debugger is attached, this method only swallows <see cref="CliFxException"/> or <see cref="CommandException"/>
        /// thrown during the actual command execution.
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
                await HandleDebugDirectiveAsync(input);

                // Preview mode
                if (HandlePreviewDirective(input))
                    return ExitCode.Success;

                var resolvedCommand = applicationSchema.Resolve(
                    input,
                    environmentVariables,
                    _typeActivator
                );

                try
                {
                    return ExitCode.Success;
                }
                // This handles both domain exceptions from CliFx as well as exceptions
                // thrown in order to short-circuit command execution due to a failure.
                catch (CliFxException ex)
                {
                    // The stack trace is irrelevant here so avoid it if possible
                    var message = ex.HasMessage
                        ? ex.Message
                        : ex.ToString();

                    WriteError(message);

                    // The exception may trigger help text to provide additional info to the user
                    if (ex.ShowHelp)
                        _helpTextWriter.Write(applicationSchema, resolvedCommand.Schema);

                    return ex.ExitCode;
                }
            }
            // To prevent the app from showing the annoying Windows troubleshooting dialog,
            // we handle all exceptions and write them to the console nicely.
            // However, we don't want to swallow unhandled exceptions when the debugger is attached,
            // because we still want the IDE to show unhandled exceptions so that the dev
            // can fix them.
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
        /// This method swallows all exceptions and routes them to the console, but only if the debugger is not attached.
        /// If the debugger is attached, this method only swallows <see cref="CliFxException"/> or <see cref="CommandException"/>
        /// thrown during the actual command execution.
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
        /// This method swallows all exceptions and routes them to the console, but only if the debugger is not attached.
        /// If the debugger is attached, this method only swallows <see cref="CliFxException"/> or <see cref="CommandException"/>
        /// thrown during the actual command execution.
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
    }
}