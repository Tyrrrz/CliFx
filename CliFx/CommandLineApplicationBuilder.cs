using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CliFx.Binding;
using CliFx.Infrastructure;
using CliFx.Utils.Extensions;

namespace CliFx;

/// <summary>
/// Builder that simplifies the creation of a <see cref="CommandLineApplication" /> instance.
/// </summary>
public partial class CommandLineApplicationBuilder
{
    private readonly HashSet<CommandDescriptor> _commands = [];

    private string? _title;
    private string? _executableName;
    private string? _version;
    private string? _description;

    private string? _debugModeEnvironmentVariable;
    private string? _previewModeEnvironmentVariable;

    private IConsole? _console;
    private ITypeActivator? _typeActivator;

    /// <summary>
    /// Registers a command with the application.
    /// </summary>
    public CommandLineApplicationBuilder AddCommand(CommandDescriptor command)
    {
        _commands.Add(command);
        return this;
    }

    /// <summary>
    /// Registers multiple commands with the application.
    /// </summary>
    public CommandLineApplicationBuilder AddCommands(IEnumerable<CommandDescriptor> commands)
    {
        foreach (var command in commands)
            AddCommand(command);

        return this;
    }

    /// <summary>
    /// Sets the application title, which is displayed in the help text.
    /// </summary>
    /// <remarks>
    /// By default, application title is inferred from the assembly name.
    /// </remarks>
    public CommandLineApplicationBuilder SetTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the application executable name, which is displayed in the help text.
    /// </summary>
    /// <remarks>
    /// By default, application executable name is inferred from the assembly file name.
    /// </remarks>
    public CommandLineApplicationBuilder SetExecutableName(string executableName)
    {
        _executableName = executableName;
        return this;
    }

    /// <summary>
    /// Sets the application version, which is displayed in the help text or when the version information is requested.
    /// </summary>
    /// <remarks>
    /// By default, application version is inferred from the assembly version.
    /// </remarks>
    public CommandLineApplicationBuilder SetVersion(string version)
    {
        _version = version;
        return this;
    }

    /// <summary>
    /// Sets the application description, which is displayed in the help text.
    /// </summary>
    public CommandLineApplicationBuilder SetDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Enables the debug mode, activated when the specified environment variable is set to <c>true</c>.
    /// When active, the application waits for a debugger to attach before executing the command.
    /// </summary>
    public CommandLineApplicationBuilder AllowDebugMode(string? environmentVariableName)
    {
        _debugModeEnvironmentVariable = environmentVariableName;
        return this;
    }

    /// <summary>
    /// Enables the preview mode, activated when the specified environment variable is set to <c>true</c>.
    /// When active, the application prints the parsed command line before executing the command.
    /// </summary>
    public CommandLineApplicationBuilder AllowPreviewMode(string? environmentVariableName)
    {
        _previewModeEnvironmentVariable = environmentVariableName;
        return this;
    }

    /// <summary>
    /// Configures the application to use the specified implementation of <see cref="IConsole" />.
    /// </summary>
    public CommandLineApplicationBuilder UseConsole(IConsole console)
    {
        _console = console;
        return this;
    }

    /// <summary>
    /// Configures the application to use the specified implementation of <see cref="ITypeActivator" />.
    /// </summary>
    public CommandLineApplicationBuilder UseTypeActivator(ITypeActivator typeActivator)
    {
        _typeActivator = typeActivator;
        return this;
    }

    /// <summary>
    /// Configures the application to use the specified delegate for activating types.
    /// </summary>
    public CommandLineApplicationBuilder UseTypeActivator(Func<Type, object> createInstance) =>
        UseTypeActivator(new DelegateTypeActivator(createInstance));

    /// <summary>
    /// Configures the application to use the specified service provider for activating types.
    /// </summary>
    public CommandLineApplicationBuilder UseTypeActivator(IServiceProvider serviceProvider) =>
        // Null returns are handled by DelegateTypeActivator
        UseTypeActivator(serviceProvider.GetService!);

    /// <summary>
    /// Configures the application to use the specified service provider for activating types.
    /// This method takes a delegate that receives the list of all added commands, so that you can
    /// easily register their types with the service provider.
    /// </summary>
    public CommandLineApplicationBuilder UseTypeActivator(
        Func<IReadOnlyList<CommandDescriptor>, IServiceProvider> getServiceProvider
    ) => UseTypeActivator(getServiceProvider([.. _commands]));

    /// <summary>
    /// Creates a configured instance of <see cref="CommandLineApplication" />.
    /// </summary>
    public CommandLineApplication Build()
    {
        var metadata = new CommandLineApplicationMetadata(
            _title ?? GetDefaultTitle(),
            _executableName ?? GetDefaultExecutableName(),
            _version ?? GetDefaultVersionText(),
            _description
        );

        var configuration = new CommandLineApplicationConfiguration(
            [.. _commands],
            _debugModeEnvironmentVariable,
            _previewModeEnvironmentVariable
        );

        return new CommandLineApplication(
            metadata,
            configuration,
            _console ?? new SystemConsole(),
            _typeActivator ?? new DefaultTypeActivator()
        );
    }
}

public partial class CommandLineApplicationBuilder
{
    private static string GetDefaultTitle()
    {
        var entryAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
        if (string.IsNullOrWhiteSpace(entryAssemblyName))
        {
            throw new InvalidOperationException(
                "Failed to infer the default application title. "
                    + $"Please specify it explicitly using `{nameof(SetTitle)}()`."
            );
        }

        return entryAssemblyName;
    }

    private static string GetDefaultExecutableName()
    {
        var processFilePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(processFilePath))
        {
            throw new InvalidOperationException(
                "Failed to infer the default application executable name. "
                    + $"Please specify it explicitly using `{nameof(SetExecutableName)}()`."
            );
        }

        return Path.GetFileName(processFilePath);
    }

    private static string GetDefaultVersionText()
    {
        var entryAssemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version;
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
