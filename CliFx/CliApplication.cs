namespace CliFx
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using CliFx.Attributes;
    using CliFx.Exceptions;
    using CliFx.Input;
    using CliFx.Internal;
    using CliFx.Schemas;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Command line application facade.
    /// </summary>
    public partial class CliApplication
    {
        /// <summary>
        /// Services provider.
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Service scope factory.
        /// <remarks>
        /// A scope is defined as a lifetime of a command execution pipeline that includes directives handling.
        /// </remarks>
        /// </summary>
        protected IServiceScopeFactory ServiceScopeFactory { get; }

        /// <summary>
        /// Cli Context instance.
        /// </summary>
        protected CliContext CliContext { get; }

        private readonly ApplicationMetadata _metadata;
        private readonly ApplicationConfiguration _configuration;
        private readonly IConsole _console;

        private readonly HelpTextWriter _helpTextWriter;

        /// <summary>
        /// Initializes an instance of <see cref="CliApplication"/>.
        /// </summary>
        public CliApplication(IServiceProvider serviceProvider, CliContext cliContext)
        {
            ServiceProvider = serviceProvider;
            ServiceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();

            CliContext = cliContext;

            _metadata = cliContext.Metadata;
            _configuration = cliContext.Configuration;
            _console = cliContext.Console;

            _helpTextWriter = new HelpTextWriter(cliContext);
        }

        private ICommand GetCommandInstance(IServiceScope serviceScope, CommandSchema command)
        {
            return command != StubDefaultCommand.Schema ? (ICommand)serviceScope.ServiceProvider.GetRequiredService(command.Type) : new StubDefaultCommand();
        }

        private IDirective GetDirectiveInstance(IServiceScope serviceScope, DirectiveSchema directive)
        {
            return (IDirective)serviceScope.ServiceProvider.GetRequiredService(directive.Type);
        }

        /// <summary>
        /// Prints the startup message if availble.
        /// </summary>
        protected void PrintStartupMessage() //TODO remove and add mediatr like behaviours or middlewares
        {
            if (_metadata.StartupMessage is null)
                return;

            _console.WithForegroundColor(ConsoleColor.Blue, () => _console.Output.WriteLine(_metadata.StartupMessage));
        }

        /// <summary>
        /// Prints the exit message if availble.
        /// </summary>
        protected void PrintExitMessage(int exitCode) //TODO remove and add mediatr like behaviours or middlewares
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
            string[] commandLineArguments = Environment.GetCommandLineArgs()
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
            Dictionary<string, string> environmentVariables = Environment.GetEnvironmentVariables()
                                                                         .Cast<DictionaryEntry>()
                                                                         .ToDictionary(x => (string)x.Key,
                                                                                       x => (string)x.Value,
                                                                                       StringComparer.Ordinal);

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
                _console.ForegroundColor = ConsoleColor.Gray;

                PrintStartupMessage();

                RootSchema root = RootSchema.Resolve(_configuration.CommandTypes, _configuration.DirectiveTypes);
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
                _configuration.ExceptionHandler.HandleException(CliContext, ex);

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
            CommandInput input = CommandInput.Parse(commandLineArguments, root.GetCommandNames());
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
            // Create service scope
            using IServiceScope serviceScope = ServiceScopeFactory.CreateScope();

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
            var instance = GetCommandInstance(serviceScope, command);

            // To avoid instantiating the command twice, we need to get default values
            // before the arguments are bound to the properties
            var defaultValues = command.GetArgumentValues(instance);

            try
            {
                // Handle directives not supported in normal mode
                if (!_configuration.IsInteractiveModeAllowed && (input.HasDirective(BuiltInDirectives.Interactive)))
                    throw CliFxException.InteractiveModeNotSupported();

                if (!await ProcessDefinedDirectives(serviceScope, root, input))
                    return ExitCode.Success;
            }
            catch (DirectiveException ex)
            {
                _configuration.ExceptionHandler.HandleDirectiveException(CliContext, ex);

                if (ex.ShowHelp)
                    _helpTextWriter.Write(root, command, defaultValues);

                return ExitCode.FromException(ex);
            }
            // This may throw exceptions which are useful only to the end-user
            catch (CliFxException ex)
            {
                _configuration.ExceptionHandler.HandleCliFxException(CliContext, ex);

                if (ex.ShowHelp)
                    _helpTextWriter.Write(root, command, defaultValues);

                return ExitCode.FromException(ex);
            }

            // Help option
            if (command.IsHelpOptionAvailable && input.IsHelpOptionSpecified ||
                command == StubDefaultCommand.Schema && !input.Parameters.Any() && !input.Options.Any())
            {
                _helpTextWriter.Write(root, command, defaultValues); //TODO: add directives help
                return ExitCode.Success;
            }

            // Handle directives not supported in normal mode
            if (!_configuration.IsInteractiveModeAllowed && command.InteractiveModeOnly)
            {
                throw CliFxException.InteractiveOnlyCommandButThisIsNormalApplication(command);
            }
            else if (_configuration.IsInteractiveModeAllowed && command.InteractiveModeOnly &&
                     !(CliContext.IsInteractiveMode || input.IsInteractiveDirectiveSpecified))
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
                _configuration.ExceptionHandler.HandleCliFxException(CliContext, ex);

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
                _configuration.ExceptionHandler.HandleCommandException(CliContext, ex);

                if (ex.ShowHelp)
                    _helpTextWriter.Write(root, command, defaultValues);

                return ex.ExitCode;
            }
        }

        private async Task<bool> ProcessDefinedDirectives(IServiceScope serviceScope, RootSchema root, CommandInput input)
        {
            bool isInteractiveMode = CliContext.IsInteractiveMode;
            foreach (CommandDirectiveInput directiveInput in input.Directives)
            {
                // Try to get the directive matching the input or fallback to default
                DirectiveSchema directive = root.TryFindDirective(directiveInput.Name) ?? throw CliFxException.UnknownDirectiveName(directiveInput);

                if (!isInteractiveMode && directive.InteractiveModeOnly)
                    throw CliFxException.InteractiveModeDirectiveNotAvailable(directiveInput.Name);

                // Get directive instance
                IDirective instance = GetDirectiveInstance(serviceScope, directive);

                await instance.HandleAsync(_console);

                if (!instance.ContinueExecution)
                    return false;
            }

            return true;
        }
    }

    public partial class CliApplication
    {
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

            public ValueTask ExecuteAsync(IConsole console)
            {
                return default;
            }
        }
    }
}
