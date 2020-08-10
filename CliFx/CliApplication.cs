using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Domain;
using CliFx.Domain.Input;
using CliFx.Exceptions;
using CliFx.Internal;

namespace CliFx
{
    /// <summary>
    /// Command line application facade.
    /// </summary>
    public partial class CliApplication
    {
        /// <summary>
        /// Cli Context instance.
        /// </summary>
        protected CliContext CliContext { get; }

        private readonly ApplicationMetadata _metadata;
        private readonly ApplicationConfiguration _configuration;
        private readonly IConsole _console;
        private readonly ITypeActivator _typeActivator;

        private readonly HelpTextWriter _helpTextWriter;

        /// <summary>
        /// Initializes an instance of <see cref="CliApplication"/>.
        /// </summary>
        [Obsolete]
        public CliApplication(ApplicationMetadata metadata,
                              ApplicationConfiguration configuration,
                              IConsole console,
                              ITypeActivator typeActivator)
        {
            CliContext = new CliContext(metadata, configuration, console);

            _metadata = metadata;
            _configuration = configuration;
            _console = console;

            _typeActivator = typeActivator;
            _helpTextWriter = new HelpTextWriter(CliContext);
        }

        /// <summary>
        /// Initializes an instance of <see cref="CliApplication"/>.
        /// </summary>
        public CliApplication(CliContext cliContext, ITypeActivator typeActivator)
        {
            CliContext = cliContext;

            _metadata = cliContext.Metadata;
            _configuration = cliContext.Configuration;
            _console = cliContext.Console;

            _typeActivator = typeActivator;
            _helpTextWriter = new HelpTextWriter(cliContext);
        }

        /// <summary>
        /// Launches and waits for debugger.
        /// </summary>
        protected async ValueTask LaunchAndWaitForDebuggerAsync()
        {
            var processId = ProcessEx.GetCurrentProcessId();

            _console.WithForegroundColor(ConsoleColor.Green, () =>
                _console.Output.WriteLine($"Attach debugger to PID {processId} to continue."));

            Debugger.Launch();

            while (!Debugger.IsAttached)
                await Task.Delay(100);

            _console.WithForegroundColor(ConsoleColor.Green, () =>
                _console.Output.WriteLine($"Debugger attached to PID {processId}."));
        }

        /// <summary>
        /// Writes command line input.
        /// </summary>
        internal void WriteCommandLineInput(CommandInput input)
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
            foreach (var option in input.Options)
            {
                _console.Output.Write('[');

                _console.WithForegroundColor(ConsoleColor.White, () =>
                {
                    // Alias
                    _console.Output.Write(option.GetRawAlias());

                    // Values
                    if (option.Values.Any())
                    {
                        _console.Output.Write(' ');
                        _console.Output.Write(option.GetRawValues());
                    }
                });

                _console.Output.Write(']');
                _console.Output.Write(' ');
            }

            _console.Output.WriteLine();
        }

        private ICommand GetCommandInstance(CommandSchema command)
        {
            return command != StubDefaultCommand.Schema ? (ICommand)_typeActivator.CreateInstance(command.Type) : new StubDefaultCommand();
        }

        /// <summary>
        /// Prints the startup message if availble.
        /// </summary>
        protected void PrintStartupMessage()
        {
            if (_metadata.StartupMessage is null)
                return;

            _console.WithForegroundColor(ConsoleColor.Blue, () => _console.Output.WriteLine(_metadata.StartupMessage));
        }

        /// <summary>
        /// Prints the exit message if availble.
        /// </summary>
        protected void PrintExitMessage(int exitCode)
        {
            CommandExitMessageOptions level = _configuration.CommandExitMessageOptions;
            bool isInteractive = CliContext.IsInteractiveMode;

            if ((isInteractive && level.HasFlag(CommandExitMessageOptions.InIteractiveMode)) ||
                (!isInteractive && level.HasFlag(CommandExitMessageOptions.InNormalMode)))
            {
                if (exitCode > 0 && level.HasFlag(CommandExitMessageOptions.OnError))
                {
                    _console.WithForegroundColor(_configuration.CommandExitMessageForeground, () =>
                        _console.Output.WriteLine($"{CliContext.Metadata.ExecutableName}: Command finished with exit code ({exitCode})."));
                }
                else if (exitCode == 0 && level.HasFlag(CommandExitMessageOptions.OnSuccess))
                {
                    _console.WithForegroundColor(_configuration.CommandExitMessageForeground, () =>
                        _console.Output.WriteLine($"{CliContext.Metadata.ExecutableName}: Command finished succesfully."));
                }
            }
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
                .ToDictionary(e => (string)e.Key, e => (string)e.Value, StringComparer.Ordinal);

            return await RunAsync(commandLineArguments, environmentVariables);
        }

        /// <summary>
        /// Runs the application with specified command line arguments and environment variables, and returns the exit code.
        /// </summary>
        /// <remarks>
        /// If a <see cref="CommandException"/> or <see cref="CliFxException"/> is thrown during command execution, it will be handled and routed to the console.
        /// Additionally, if the debugger is not attached (i.e. the app is running in production), all other exceptions thrown within
        /// this method will be handled and routed to the console as well.
        /// </remarks>
        public async ValueTask<int> RunAsync(IReadOnlyList<string> commandLineArguments,
                                             IReadOnlyDictionary<string, string> environmentVariables)
        {
            try
            {
                _console.ResetColor();
                PrintStartupMessage();

                var root = RootSchema.Resolve(_configuration.CommandTypes);
                CliContext.Root = root;

                int exitCode = await PreExecuteCommand(commandLineArguments, environmentVariables, root);

                PrintExitMessage(exitCode);

                return exitCode;
            }
            // To prevent the app from showing the annoying Windows troubleshooting dialog,
            // we handle all exceptions and route them to the console nicely.
            // However, we don't want to swallow unhandled exceptions when the debugger is attached,
            // because we still want the IDE to show them to the developer.
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                _configuration.ExceptionHandler.HandleException(_console, CliContext, ex);

                return ExitCode.FromException(ex);
            }
        }

        /// <summary>
        /// Runs before command execution.
        /// </summary>
        protected virtual async Task<int> PreExecuteCommand(IReadOnlyList<string> commandLineArguments,
                                                            IReadOnlyDictionary<string, string> environmentVariables,
                                                            RootSchema root)
        {
            var input = CommandInput.Parse(commandLineArguments, root.GetCommandNames());
            CliContext.CurrentInput = input;

            return await ExecuteCommand(environmentVariables, root, input);
        }

        /// <summary>
        /// Executes command.
        /// </summary>
        protected async Task<int> ExecuteCommand(IReadOnlyDictionary<string, string> environmentVariables,
                                                 RootSchema root,
                                                 CommandInput input)
        {
            if (await ProcessDirectives(_configuration, input) is int exitCode)
                return exitCode;

            // Try to get the command matching the input or fallback to default
            CommandSchema command = root.TryFindCommand(input.CommandName) ?? StubDefaultCommand.Schema;
            CliContext.CurrentCommand = command;

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

            // If we want to throw error if `-hg`, `-h -g`, `--version -unknown` are given, we should move help handling to try statement after //Bind arguments
            // Help option
            if (command.IsHelpOptionAvailable && input.IsHelpOptionSpecified ||
                command == StubDefaultCommand.Schema && !input.Parameters.Any() && !input.Options.Any())
            {
                _helpTextWriter.Write(root, command, defaultValues);
                return ExitCode.Success;
            }

            // Handle directives not supported in normal mode
            if (!_configuration.IsInteractiveModeAllowed && command.InteractiveModeOnly)
            {
                throw CliFxException.InteractiveOnlyCommandButThisIsNormalApplication(command);
            }
            else if (_configuration.IsInteractiveModeAllowed && command.InteractiveModeOnly &&
                     !(CliContext.IsInteractiveMode || input.HasDirective(StandardDirectives.Interactive)))
            {
                throw CliFxException.InteractiveOnlyCommandButInteractiveModeNotStarted(command);
            }

            // Bind arguments
            try
            {
                command.Bind(instance, input, environmentVariables);
            }
            // This may throw exceptions which are useful only to the end-user
            catch (CliFxException ex)
            {
                _configuration.ExceptionHandler.HandleCliFxException(_console, CliContext, ex);

                if (ex.ShowHelp)
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
                _configuration.ExceptionHandler.HandleCommandException(_console, CliContext, ex);

                if (ex.ShowHelp)
                    _helpTextWriter.Write(root, command, defaultValues);

                return ex.ExitCode;
            }
        }

        /// <summary>
        /// Process directives.
        /// </summary>
        protected virtual async Task<int?> ProcessDirectives(ApplicationConfiguration configuration, CommandInput input)
        {
            // Debug mode
            if (configuration.IsDebugModeAllowed && input.HasDirective(StandardDirectives.Debug))
            {
                await LaunchAndWaitForDebuggerAsync();
            }

            // Preview mode
            if (configuration.IsPreviewModeAllowed && input.HasDirective(StandardDirectives.Preview))
            {
                WriteCommandLineInput(input);
                return ExitCode.Success;
            }

            // Handle directives not supported in normal mode
            if (!configuration.IsInteractiveModeAllowed &&
                (input.HasDirective(StandardDirectives.Interactive) || input.HasAnyOfDirectives(InteractiveModeDirectives)))
            {
                throw CliFxException.InteractiveModeDirectivesNotSupported();
            }

            return null;
        }
    }

    public partial class CliApplication
    {
        /// <summary>
        /// Interactive mode directives <see cref="StandardDirectives"/>.
        /// </summary>
        internal static string[] InteractiveModeDirectives = new string[]
        {
            StandardDirectives.Default,
            StandardDirectives.Scope,
            StandardDirectives.ScopeUp,
            StandardDirectives.ScopeReset
        };

        /// <summary>
        /// Static exit codes helper class.
        /// </summary>
        protected static class ExitCode
        {
            /// <summary>
            /// Success exit code.
            /// </summary>
            public const int Success = 0;

            /// <summary>
            /// Error exit code.
            /// </summary>
            public const int Error = 1;

            /// <summary>
            /// Gets an exit code from exception.
            /// </summary>
            public static int FromException(Exception ex)
            {
                return ex is CommandException cmdEx ? cmdEx.ExitCode : Error;
            }
        }

        [Command]
        private class StubDefaultCommand : ICommand
        {
            public static CommandSchema Schema { get; } = CommandSchema.TryResolve(typeof(StubDefaultCommand))!;

            public ValueTask ExecuteAsync(IConsole console) => default;
        }
    }
}
