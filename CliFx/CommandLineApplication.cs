using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Activation;
using CliFx.Binding;
using CliFx.Help;
using CliFx.Infrastructure;
using CliFx.Parsing;
using CliFx.Utils.Extensions;

namespace CliFx;

/// <summary>
/// Command-line application facade.
/// </summary>
public class CommandLineApplication(
    CommandLineApplicationMetadata metadata,
    CommandLineApplicationConfiguration configuration,
    IConsole console,
    ITypeInstantiator typeInstantiator
)
{
    private async ValueTask<int> RunAsync(
        CommandRootDescriptor root,
        ParsedCommandLine commandLine,
        IReadOnlyDictionary<string, string> environmentVariables
    )
    {
        console.ResetColor();

        // Handle debug mode
        if (
            !string.IsNullOrWhiteSpace(configuration.DebugModeEnvironmentVariable)
            && bool.ParseOrDefault(
                environmentVariables.GetValueOrDefault(configuration.DebugModeEnvironmentVariable)
            )
        )
        {
            using (console.WithForegroundColor(ConsoleColor.Green))
            {
                console.WriteLine(
                    $"Attach the debugger to process with ID {Environment.ProcessId} to continue."
                );
            }

            // Try to also launch the debugger ourselves (only works with Visual Studio)
            Debugger.Launch();

            // Wait for the debugger to attach
            await Debugger.WaitUntilAttachedAsync();
        }

        // Handle preview mode
        if (
            !string.IsNullOrWhiteSpace(configuration.PreviewModeEnvironmentVariable)
            && bool.ParseOrDefault(
                environmentVariables.GetValueOrDefault(configuration.PreviewModeEnvironmentVariable)
            )
        )
        {
            console.WriteCommandLine(commandLine);
        }

        // Try to find the command that matches the input
        var command =
            (
                !string.IsNullOrWhiteSpace(commandLine.CommandName)
                    // If the command name is specified, try to find the command by name.
                    // This should always succeed, because the input parsing relies on
                    // the list of available command names.
                    ? root.TryFindCommand(commandLine.CommandName)
                    // Otherwise, try to find the default command
                    : root.TryFindDefaultCommand()
            )
            ??
            // If a valid command was not found, use the fallback default command
            FallbackDefaultCommand.Descriptor;

        // Initialize an instance of the command
        var instance =
            command != FallbackDefaultCommand.Descriptor
                ? typeInstantiator.CreateInstance(command)
                // Bypass the instantiator
                : new FallbackDefaultCommand();

        // Assemble the help context
        var helpContext = new HelpContext(
            metadata,
            root,
            command,
            command.Inputs.ToDictionary(i => i, i => i.Property.GetValue(instance))
        );

        // Assemble the command activator
        var activator = new CommandActivator(command, instance, environmentVariables);

        // Perform a limited command activation only for the conventional help and version options,
        // so that they can be handled even if the rest of the command-line input is invalid.
        activator.ActivateHelpAndVersionOptions(commandLine);

        // Handle help option
        if (instance is ICommandWithHelpOption { IsHelpRequested: true })
        {
            console.WriteHelp(helpContext);
            return 0;
        }

        // Handle version option
        if (instance is ICommandWithVersionOption { IsVersionRequested: true })
        {
            console.WriteLine(metadata.Version);
            return 0;
        }

        // Starting from this point, we may produce exceptions that are meant for the
        // end-user of the application (i.e. invalid input, command exception, etc.).
        // Catch these exceptions here, print them to the console, and don't let them
        // propagate further.
        try
        {
            // Activate the command inputs from the command line
            activator.Activate(commandLine);

            // Execute the command
            await instance.ExecuteAsync(console);

            return 0;
        }
        catch (CliFxException ex)
        {
            console.WriteException(ex);

            if (ex.ShowHelp)
            {
                console.WriteLine();
                console.WriteHelp(helpContext);
            }

            return ex.ExitCode;
        }
    }

    /// <summary>
    /// Runs the application with the specified command-line arguments and environment variables.
    /// Returns the exit code which indicates whether the application completed successfully.
    /// </summary>
    /// <remarks>
    /// When running WITHOUT the debugger attached (i.e. in production), this method swallows
    /// all inner exceptions and reports them to the console, instead of allowing them to propagate
    /// to the caller and potentially crash the application.
    /// </remarks>
    public async ValueTask<int> RunAsync(
        IReadOnlyList<string> commandLineArguments,
        IReadOnlyDictionary<string, string> environmentVariables
    )
    {
        try
        {
            var root = new CommandRootDescriptor(configuration.Commands);
            var commandLine = ParsedCommandLine.Parse(commandLineArguments, root.GetCommandNames());

            return await RunAsync(root, commandLine, environmentVariables);
        }
        // To prevent the app from showing the annoying troubleshooting dialog on Windows,
        // we handle all exceptions ourselves and print them to the console.
        // We only want to do that if the app is running in production, which we infer
        // based on whether the debugger is attached to the process.
        // When not running in production, we want the IDE to show exceptions to the
        // developer, so we don't swallow them in that case.
        catch (Exception ex) when (!Debugger.IsAttached)
        {
            console.WriteException(ex);
            return 1;
        }
    }

    /// <summary>
    /// Runs the application with the specified command-line arguments, while the environment variables
    /// are resolved automatically from <see cref="Environment" />.
    /// Returns the exit code which indicates whether the application completed successfully.
    /// </summary>
    /// <remarks>
    /// When running WITHOUT the debugger attached (i.e. in production), this method swallows
    /// all inner exceptions and reports them to the console, instead of allowing them to propagate
    /// to the caller and potentially crash the application.
    /// </remarks>
    public async ValueTask<int> RunAsync(IReadOnlyList<string> commandLineArguments) =>
        await RunAsync(
            commandLineArguments,
            Environment
                .GetEnvironmentVariables()
                .ToDictionary<string, string>(StringComparer.Ordinal)
        );

    /// <summary>
    /// Runs the application.
    /// Command-line arguments and environment variables are resolved automatically from <see cref="Environment" />.
    /// Returns the exit code which indicates whether the application completed successfully.
    /// </summary>
    /// <remarks>
    /// When running WITHOUT the debugger attached (i.e. in production), this method swallows
    /// all inner exceptions and reports them to the console, instead of allowing them to propagate
    /// to the caller and potentially crash the application.
    /// </remarks>
    public async ValueTask<int> RunAsync() =>
        await RunAsync(
            Environment
                .GetCommandLineArgs()
                // First element is the file path
                .Skip(1)
                .ToArray()
        );
}
