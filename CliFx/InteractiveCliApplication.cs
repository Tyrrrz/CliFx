using System;
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
    public partial class InteractiveCliApplication : CliApplication
    {
        private readonly ConsoleColor _promptForeground = ConsoleColor.Blue;
        private readonly ConsoleColor _commandForeground = ConsoleColor.Yellow;
        private readonly ConsoleColor _finishedResultForeground = ConsoleColor.White;

        /// <summary>
        /// Initializes an instance of <see cref="InteractiveCliApplication"/>.
        /// </summary>
        public InteractiveCliApplication(CliContext cliContext, ITypeActivator typeActivator) :
            base(cliContext, typeActivator)
        {

        }

        /// <summary>
        /// Runs the application with specified command line arguments and environment variables, and returns the exit code.
        /// </summary>
        /// <remarks>
        /// If a <see cref="CommandException"/> or <see cref="CliFxException"/> is thrown during command execution, it will be handled and routed to the console.
        /// Additionally, if the debugger is not attached (i.e. the app is running in production), all other exceptions thrown within
        /// this method will be handled and routed to the console as well.
        /// </remarks>
        public override async ValueTask<int> RunAsync(IReadOnlyList<string> commandLineArguments,
                                                      IReadOnlyDictionary<string, string> environmentVariables)
        {
            ApplicationConfiguration _configuration = CliContext.Configuration;
            IConsole _console = CliContext.Console;
            _console.ForegroundColor = ConsoleColor.Gray;

            try
            {
                var root = RootSchema.Resolve(_configuration.CommandTypes);
                var input = CommandInput.Parse(commandLineArguments, root.GetCommandNames());

                bool isInteractiveMode = input.HasDirective(StandardDirectives.Interactive);

                // Debug mode
                if (_configuration.IsDebugModeAllowed && input.HasDirective(StandardDirectives.Debug))
                {
                    await LaunchAndWaitForDebuggerAsync();
                }

                // Preview mode
                if (_configuration.IsPreviewModeAllowed && input.HasDirective(StandardDirectives.Preview))
                {
                    WriteCommandLineInput(input);
                    return ExitCode.Success;
                }

                if (isInteractiveMode)
                {
                    CliContext.IsInteractive = true;

                    await ProcessCommand(commandLineArguments, environmentVariables, root, input);
                    await RunInteractivelyAsync(environmentVariables, _console, root, _configuration);
                }

                return await ProcessCommand(commandLineArguments, environmentVariables, root, input);
            }
            // To prevent the app from showing the annoying Windows troubleshooting dialog,
            // we handle all exceptions and route them to the console nicely.
            // However, we don't want to swallow unhandled exceptions when the debugger is attached,
            // because we still want the IDE to show them to the developer.
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                _configuration.ExceptionHandler.HandleException(_console, ex);

                return ExitCode.FromException(ex);
            }
        }

        private async ValueTask<int> RunInteractivelyAsync(IReadOnlyDictionary<string, string> environmentVariables,
                                                           IConsole _console,
                                                           RootSchema root,
                                                           ApplicationConfiguration _configuration)
        {
            //TODO: Add startup message or add behaviours like in mediatr
            while (true) //TODO maybe add CliContext.Exit and CliContext.Status
            {
                //TODO add directives checking

                string[] commandLineArguments = GetInput(_console, CliContext.Metadata.ExecutableName);

                var input = CommandInput.Parse(commandLineArguments, root.GetCommandNames());

                int exitCode = await ProcessCommand(commandLineArguments, environmentVariables, root, input);

                _console.WithForegroundColor(_finishedResultForeground, () =>
                {
                    //if (exitCode == 0)
                    //    _console.Output.WriteLine($"{CliContext.Metadata.ExecutableName}: Command finished succesfully");
                    //else
                    _console.Output.WriteLine($"{CliContext.Metadata.ExecutableName}: Command finished with exit code ({exitCode})");
                });
            }

            //return ExitCode.Success;
        }

        private string[] GetInput(IConsole _console, string executableName)
        {
            string[] arguments;
            string line = string.Empty;
            do
            {
                _console.WithForegroundColor(_promptForeground, () =>
                {
                    _console.Output.Write(executableName);
                });

                if (!string.IsNullOrWhiteSpace(CliContext.Scope))
                {
                    _console.WithForegroundColor(ConsoleColor.Cyan, () =>
                    {
                        _console.Output.Write(CliContext.Scope);
                    });
                }

                _console.WithForegroundColor(_promptForeground, () =>
                {
                    _console.Output.Write("> ");
                });

                _console.WithForegroundColor(_commandForeground, () =>
                {
                    line = _console.Input.ReadLine();
                });

                if (line.StartsWith(StandardDirectives.Default))
                    return Array.Empty<string>();

#if NETSTANDARD2_0
                arguments = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
#else
                arguments = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToArray();
#endif

            } while (string.IsNullOrWhiteSpace(line));

            return arguments;
        }
    }
}
