using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Exceptions;
using CliFx.Formatting;
using CliFx.Infrastructure;
using CliFx.Input;
using CliFx.Schema;
using CliFx.Utils;
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

    private bool IsDebugModeEnabled(CommandInput commandInput) =>
        Configuration.IsDebugModeAllowed && commandInput.IsDebugDirectiveSpecified;

    private bool IsPreviewModeEnabled(CommandInput commandInput) =>
        Configuration.IsPreviewModeAllowed && commandInput.IsPreviewDirectiveSpecified;

    private async ValueTask PromptDebuggerAsync()
    {
        using (console.WithForegroundColor(ConsoleColor.Green))
        {
            console.Output.WriteLine(
                $"Attach the debugger to process with ID {ProcessEx.GetCurrentProcessId()} to continue."
            );
        }

        // Try to also launch the debugger ourselves (only works with Visual Studio)
        Debugger.Launch();

        while (!Debugger.IsAttached)
            await Task.Delay(100);
    }

    private async ValueTask<int> RunAsync(
        ApplicationSchema applicationSchema,
        CommandInput commandInput
    )
    {
        // Console colors may have already been overridden by the parent process,
        // so we need to reset it to make sure that everything we write looks properly.
        console.ResetColor();

        // Handle the debug directive
        if (IsDebugModeEnabled(commandInput))
        {
            await PromptDebuggerAsync();
        }

        // Handle the preview directive
        if (IsPreviewModeEnabled(commandInput))
        {
            console.WriteCommandInput(commandInput);
            return 0;
        }

        // Try to get the command schema that matches the input
        var commandSchema =
            (
                !string.IsNullOrWhiteSpace(commandInput.CommandName)
                    // If the command name is specified, try to find the command by name.
                    // This should always succeed, because the input parsing relies on
                    // the list of available command names.
                    ? applicationSchema.TryFindCommand(commandInput.CommandName)
                    // Otherwise, try to find the default command
                    : applicationSchema.TryFindDefaultCommand()
            )
            ??
            // If a valid command was not found, use the fallback default command.
            // This is only used as a stub to show the help text.
            FallbackDefaultCommand.Schema;

        // Initialize an instance of the command type
        var commandInstance =
            commandSchema == FallbackDefaultCommand.Schema
                ? new FallbackDefaultCommand() // bypass the activator
                : typeActivator.CreateInstance<ICommand>(commandSchema.Type);

        // Assemble the help context
        var helpContext = new HelpContext(
            Metadata,
            applicationSchema,
            commandSchema,
            commandSchema.GetValues(commandInstance)
        );

        // Starting from this point, we may produce exceptions that are meant for the
        // end-user of the application (i.e. invalid input, command exception, etc).
        // Catch these exceptions here, print them to the console, and don't let them
        // propagate further.
        try
        {
            // Activate the command instance with the provided input
            commandSchema.Activate(commandInstance, commandInput);

            // Handle the version option
            if (commandInstance is ICommandWithVersionOption { IsVersionRequested: true })
            {
                console.WriteLine(Metadata.Version);
                return 0;
            }

            // Handle the help option
            if (
                commandInstance
                is ICommandWithHelpOption { IsHelpRequested: true }
                    // Fallback default command always shows help, even if the option is not specified
                    or FallbackDefaultCommand
            )
            {
                console.WriteHelpText(helpContext);
                return 0;
            }

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
            return await RunAsync(
                Configuration.Schema,
                CommandInput.Parse(
                    commandLineArguments,
                    environmentVariables,
                    Configuration.Schema.GetCommandNames()
                )
            );
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
                .Skip(1) // first element is the file path
                .ToArray()
        );
}
