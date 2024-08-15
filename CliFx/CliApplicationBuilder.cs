using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using CliFx.Infrastructure;
using CliFx.Schema;
using CliFx.Utils;
using CliFx.Utils.Extensions;

namespace CliFx;

/// <summary>
/// Builder for <see cref="CliApplication" />.
/// </summary>
public partial class CliApplicationBuilder
{
    private readonly HashSet<CommandSchema> _commands = [];

    private bool _isDebugModeAllowed = true;
    private bool _isPreviewModeAllowed = true;
    private string? _title;
    private string? _executableName;
    private string? _version;
    private string? _description;
    private IConsole? _console;
    private ITypeActivator? _typeActivator;

    // TODO:
    // The source generator should generate an internal extension method for the builder called
    // AddCommandsFromThisAssembly() that would add all command types from the assembly where the builder is used.

    /// <summary>
    /// Adds a command to the application.
    /// </summary>
    public CliApplicationBuilder AddCommand(CommandSchema command)
    {
        _commands.Add(command);
        return this;
    }

    /// <summary>
    /// Adds multiple commands to the application.
    /// </summary>
    public CliApplicationBuilder AddCommands(IReadOnlyList<CommandSchema> commands)
    {
        foreach (var command in commands)
            AddCommand(command);

        return this;
    }

    /// <summary>
    /// Specifies whether debug mode (enabled with the [debug] directive) is allowed in the application.
    /// </summary>
    public CliApplicationBuilder AllowDebugMode(bool isAllowed = true)
    {
        _isDebugModeAllowed = isAllowed;
        return this;
    }

    /// <summary>
    /// Specifies whether preview mode (enabled with the [preview] directive) is allowed in the application.
    /// </summary>
    public CliApplicationBuilder AllowPreviewMode(bool isAllowed = true)
    {
        _isPreviewModeAllowed = isAllowed;
        return this;
    }

    /// <summary>
    /// Sets the application title, which is shown in the help text.
    /// </summary>
    /// <remarks>
    /// By default, application title is inferred from the assembly name.
    /// </remarks>
    public CliApplicationBuilder SetTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the application executable name, which is shown in the help text.
    /// </summary>
    /// <remarks>
    /// By default, application executable name is inferred from the assembly file name.
    /// </remarks>
    public CliApplicationBuilder SetExecutableName(string executableName)
    {
        _executableName = executableName;
        return this;
    }

    /// <summary>
    /// Sets the application version, which is shown in the help text or when the user specifies the version option.
    /// </summary>
    /// <remarks>
    /// By default, application version is inferred from the assembly version.
    /// </remarks>
    public CliApplicationBuilder SetVersion(string version)
    {
        _version = version;
        return this;
    }

    /// <summary>
    /// Sets the application description, which is shown in the help text.
    /// </summary>
    public CliApplicationBuilder SetDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Configures the application to use the specified implementation of <see cref="IConsole" />.
    /// </summary>
    public CliApplicationBuilder UseConsole(IConsole console)
    {
        _console = console;
        return this;
    }

    /// <summary>
    /// Configures the application to use the specified implementation of <see cref="ITypeActivator" />.
    /// </summary>
    public CliApplicationBuilder UseTypeActivator(ITypeActivator typeActivator)
    {
        _typeActivator = typeActivator;
        return this;
    }

    /// <summary>
    /// Configures the application to use the specified delegate for activating types.
    /// </summary>
    public CliApplicationBuilder UseTypeActivator(Func<Type, object> createInstance) =>
        UseTypeActivator(new DelegateTypeActivator(createInstance));

    /// <summary>
    /// Configures the application to use the specified service provider for activating types.
    /// </summary>
    public CliApplicationBuilder UseTypeActivator(IServiceProvider serviceProvider) =>
        // Null returns are handled by DelegateTypeActivator
        UseTypeActivator(serviceProvider.GetService!);

    /// <summary>
    /// Creates a configured instance of <see cref="CliApplication" />.
    /// </summary>
    public CliApplication Build()
    {
        var metadata = new ApplicationMetadata(
            _title ?? GetDefaultTitle(),
            _executableName ?? GetDefaultExecutableName(),
            _version ?? GetDefaultVersionText(),
            _description
        );

        var configuration = new ApplicationConfiguration(
            new ApplicationSchema(_commands.ToArray()),
            _isDebugModeAllowed,
            _isPreviewModeAllowed
        );

        return new CliApplication(
            metadata,
            configuration,
            _console ?? new SystemConsole(),
            _typeActivator ?? new DefaultTypeActivator()
        );
    }
}

public partial class CliApplicationBuilder
{
    private static string GetDefaultTitle()
    {
        var entryAssemblyName = EnvironmentEx.EntryAssembly?.GetName().Name;
        if (string.IsNullOrWhiteSpace(entryAssemblyName))
        {
            throw new InvalidOperationException(
                "Failed to infer the default application title. "
                    + $"Please specify it explicitly using `{nameof(SetTitle)}()`."
            );
        }

        return entryAssemblyName;
    }

    [UnconditionalSuppressMessage(
        "SingleFile",
        "IL3000:Avoid accessing Assembly file path when publishing as a single file",
        Justification = "The file path is checked to ensure the assembly location is available."
    )]
    private static string GetDefaultExecutableName()
    {
        var processFilePath = EnvironmentEx.ProcessPath;

        // Process file path should generally always be available
        if (string.IsNullOrWhiteSpace(processFilePath))
        {
            throw new InvalidOperationException(
                "Failed to infer the default application executable name. "
                    + $"Please specify it explicitly using `{nameof(SetExecutableName)}()`."
            );
        }

        var entryAssemblyFilePath = EnvironmentEx.EntryAssembly?.Location;

        // Single file application: entry assembly is not on disk and doesn't have a file path
        if (string.IsNullOrWhiteSpace(entryAssemblyFilePath))
        {
            return Path.GetFileNameWithoutExtension(processFilePath);
        }

        // Legacy .NET Framework application: entry assembly has the same file path as the process
        if (PathEx.AreEqual(entryAssemblyFilePath, processFilePath))
        {
            return Path.GetFileNameWithoutExtension(entryAssemblyFilePath);
        }

        // .NET Core application launched through the native application host:
        // entry assembly has the same file path as the process, but with a different extension.
        if (
            PathEx.AreEqual(Path.ChangeExtension(entryAssemblyFilePath, "exe"), processFilePath)
            || PathEx.AreEqual(
                Path.GetFileNameWithoutExtension(entryAssemblyFilePath),
                processFilePath
            )
        )
        {
            return Path.GetFileNameWithoutExtension(entryAssemblyFilePath);
        }

        // .NET Core application launched through the .NET CLI
        return "dotnet " + Path.GetFileName(entryAssemblyFilePath);
    }

    private static string GetDefaultVersionText()
    {
        var entryAssemblyVersion = EnvironmentEx.EntryAssembly?.GetName().Version;
        if (entryAssemblyVersion is null)
        {
            throw new InvalidOperationException(
                "Failed to infer the default application version. "
                    + $"Please specify it explicitly using `{nameof(SetVersion)}()`."
            );
        }

        return "v" + entryAssemblyVersion.ToSemanticString();
    }
}
