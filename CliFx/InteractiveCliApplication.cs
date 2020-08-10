using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Domain;
using CliFx.Domain.Input;
using CliFx.Exceptions;

namespace CliFx
{
    /// <summary>
    /// Command line application facade.
    /// </summary>
    public partial class InteractiveCliApplication : CliApplication
    {
        private readonly ConsoleColor _promptForeground;
        private readonly ConsoleColor _commandForeground;
        private readonly ConsoleColor _finishedResultForeground;

        /// <summary>
        /// Initializes an instance of <see cref="InteractiveCliApplication"/>.
        /// </summary>
        public InteractiveCliApplication(CliContext cliContext,
                                         ITypeActivator typeActivator,
                                         ConsoleColor promptForeground,
                                         ConsoleColor commandForeground,
                                         ConsoleColor finishedResultForeground) :
            base(cliContext, typeActivator)
        {
            _promptForeground = promptForeground;
            _commandForeground = commandForeground;
            _finishedResultForeground = finishedResultForeground;
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
            PrintStartupMessage();

            ApplicationConfiguration _configuration = CliContext.Configuration;
            IConsole _console = CliContext.Console;
            _console.ForegroundColor = ConsoleColor.Gray;

            try
            {
                var root = RootSchema.Resolve(_configuration.CommandTypes);
                CliContext.Root = root;

                var input = CommandInput.Parse(commandLineArguments, root.GetCommandNames());

                bool isInteractiveMode = input.HasDirective(StandardDirectives.Interactive);

                if (await ProcessDirectives(_configuration, input) is int retVal)
                    return retVal;

                if (isInteractiveMode)
                {
                    CliContext.IsInteractive = true;

                    // we don't want to run default command for e.g. `[interactive]`
                    if (!string.IsNullOrWhiteSpace(input.CommandName))
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

        private async Task<int?> ProcessDirectives(ApplicationConfiguration _configuration, CommandInput input)
        {
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

            // Scope
            if (input.HasDirective(StandardDirectives.Scope))
            {
                CliContext.Scope = input.CommandName ?? string.Empty;

                return ExitCode.Success;
            }

            // Scope reset
            if (input.HasDirective(StandardDirectives.ScopeReset))
            {
                CliContext.Scope = string.Empty;

                return ExitCode.Success;
            }

            // Scope up
            if (input.HasDirective(StandardDirectives.ScopeUp))
            {
                string[] splittedScope = CliContext.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (splittedScope.Length > 1)
                    CliContext.Scope = string.Join(" ", splittedScope, 0, splittedScope.Length - 1);
                else if (splittedScope.Length == 1)
                    CliContext.Scope = string.Empty;

                return ExitCode.Success;
            }

            return null;
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

                int? exitCode = await ProcessDirectives(_configuration, input);
                exitCode ??= await ProcessCommand(commandLineArguments, environmentVariables, root, input);

                _console.WithForegroundColor(_finishedResultForeground, () =>
                {
                    //if (exitCode == 0)
                    //    _console.Output.WriteLine($"{CliContext.Metadata.ExecutableName}: Command finished succesfully");
                    //else
                    if (exitCode > 0)
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
                        _console.Output.Write(' ');
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

                if (string.IsNullOrWhiteSpace(CliContext.Scope))
                {
                    arguments = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                    .ToArray();
                }
                else
                {
                    var tmp = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                  .ToList();

                    int lastDirective = tmp.FindLastIndex(x => x.StartsWith('[') && x.EndsWith(']'));
                    tmp.Insert(lastDirective + 1, CliContext.Scope);

                    arguments = tmp.ToArray();
                }

            } while (string.IsNullOrWhiteSpace(line));

            return arguments;
        }
    }
}
