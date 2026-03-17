using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Activation;
using CliFx.Binding;
using CliFx.Formatting;
using CliFx.Infrastructure;
using CliFx.Parsing;
using CliFx.Utils.Extensions;

namespace CliFx;

/// <summary>
/// Command-line application facade.
/// </summary>
public class CliApplication(
    ApplicationMetadata metadata,
    ApplicationConfiguration configuration,
    IConsole console,
    ITypeActivator typeActivator
)
{
    /// <summary>
    /// Application metadata.
    /// </summary>
    public ApplicationMetadata Metadata { get; } = metadata;

    /// <summary>
    /// Application configuration.
    /// </summary>
    public ApplicationConfiguration Configuration { get; } = configuration;

    private async ValueTask PromptDebuggerAsync()
    {
        using (console.WithForegroundColor(ConsoleColor.Green))
        {
            console.WriteLine(
                $"Attach the debugger to process with ID {Environment.ProcessId} to continue."
            );
        }

        // Try to also launch the debugger ourselves (only works with Visual Studio)
        Debugger.Launch();

        while (!Debugger.IsAttached)
            await Task.Delay(100);
    }

    // WriteHelpText uses TryGetValidValues() which relies on reflection for enum valid values display.
    // This is suppressed here because the public RunAsync is the user-facing entry point and marking it
    // with [RequiresUnreferencedCode] would be too disruptive. For full AOT compatibility, the source
    // generator should emit valid enum values statically (see PropertyDescriptor.TryGetValidValues).
#pragma warning disable IL2026
    private async ValueTask<int> RunAsync(
        ApplicationDescriptor applicationDescriptor,
        ParsedCommandLine commandLine
    )
    {
        console.ResetColor();

        // Handle the debug directive
        if (Configuration.IsDebugModeAllowed && commandLine.IsDebugDirectiveSpecified)
        {
            await PromptDebuggerAsync();
        }

        // Handle the preview directive
        if (Configuration.IsPreviewModeAllowed && commandLine.IsPreviewDirectiveSpecified)
        {
            console.WriteCommandInput(commandLine);
            return 0;
        }

        // Try to get the command schema that matches the input
        var commandDescriptor =
            (
                !string.IsNullOrWhiteSpace(commandLine.CommandName)
                    // If the command name is specified, try to find the command by name.
                    // This should always succeed, because the input parsing relies on
                    // the list of available command names.
                    ? applicationDescriptor.TryFindCommand(commandLine.CommandName)
                    // Otherwise, try to find the default command
                    : applicationDescriptor.TryFindDefaultCommand()
            )
            ??
            // If a valid command was not found, use the fallback default command.
            // This is only used as a stub to show the help text.
            FallbackDefaultCommand.Descriptor;

        // Initialize an instance of the command type
        var commandInstance =
            commandDescriptor != FallbackDefaultCommand.Descriptor
                ? typeActivator.CreateInstance(commandDescriptor)
                // Bypass the activator
                : new FallbackDefaultCommand();

        // Assemble the help context
        var helpContext = new HelpContext(
            Metadata,
            applicationDescriptor,
            commandDescriptor,
            commandDescriptor.Inputs.ToDictionary(
                inputDescriptor => inputDescriptor,
                inputDescriptor => inputDescriptor.Property.GetValue(commandInstance)
            )
        );

        // Assemble the command activator
        var commandActivator = new CommandActivator(commandDescriptor, commandInstance);

        // Perform a limited command activation to check if the help or version options were specified by the user
        if (commandInstance is ICommandWithHelpOption or ICommandWithVersionOption)
        {
            commandActivator.ActivateHelpAndVersionOptions(commandLine);

            // Help text
            if (
                commandInstance is ICommandWithHelpOption { IsHelpRequested: true }
                // Can also be requested on the fallback command by not supplying any arguments
                || commandDescriptor == FallbackDefaultCommand.Descriptor
                    && !commandLine.HasArguments
            )
            {
                console.WriteHelpText(helpContext);
                return 0;
            }

            // Version text
            if (commandInstance is ICommandWithVersionOption { IsVersionRequested: true })
            {
                console.WriteLine(Metadata.Version);
                return 0;
            }
        }

        // Starting from this point, we may produce exceptions that are meant for the
        // end-user of the application (i.e. invalid input, command exception, etc.).
        // Catch these exceptions here, print them to the console, and don't let them
        // propagate further.
        try
        {
            // Activate the command inputs from the command line
            commandActivator.Activate(commandLine);

            // Execute the command
            await commandInstance.ExecuteAsync(console);

            return 0;
        }
        catch (CliFxException ex)
        {
            console.WriteException(ex);

            if (ex.ShowHelp)
            {
                console.WriteLine();
                console.WriteHelpText(helpContext);
            }

            return ex.ExitCode;
        }
    }
#pragma warning restore IL2026

    /// <summary>
    /// Runs the application with the specified command-line arguments and environment variables.
    /// Returns the exit code which indicates whether the application completed successfully.
    /// </summary>
    /// <remarks>
    /// When running WITHOUT the debugger attached (i.e. in production), this method swallows
    /// all exceptions and reports them to the console.
    /// </remarks>
    public async ValueTask<int> RunAsync(
        IReadOnlyList<string> commandLineArguments,
        IReadOnlyDictionary<string, string> environmentVariables
    )
    {
        try
        {
            var applicationDescriptor = new ApplicationDescriptor(Configuration.CommandDescriptors);

            var commandLine = ParsedCommandLine.Parse(
                commandLineArguments,
                environmentVariables,
                applicationDescriptor.GetCommandNames()
            );

            return await RunAsync(applicationDescriptor, commandLine);
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
    /// Runs the application with the specified command-line arguments.
    /// Returns the exit code which indicates whether the application completed successfully.
    /// </summary>
    /// <remarks>
    /// When running WITHOUT the debugger attached (i.e. in production), this method swallows
    /// all exceptions and reports them to the console.
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
    /// Command-line arguments and environment variables are resolved automatically.
    /// Returns the exit code which indicates whether the application completed successfully.
    /// </summary>
    /// <remarks>
    /// When running WITHOUT the debugger attached (i.e. in production), this method swallows
    /// all exceptions and reports them to the console.
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
