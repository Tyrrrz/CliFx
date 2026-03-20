using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CliFx.Binding;
using CliFx.Infrastructure;
using CliFx.Utils.Extensions;

namespace CliFx;

/// <summary>
/// Builder for <see cref="CliApplication" />.
/// </summary>
public partial class CliApplicationBuilder
{
    private readonly HashSet<CommandDescriptor> _commandDescriptors = [];

    private string? _debugModeEnvironmentVariable;
    private string? _previewModeEnvironmentVariable;
    private string? _title;
    private string? _executableName;
    private string? _version;
    private string? _description;
    private IConsole? _console;
    private ITypeActivator? _typeActivator;

    /// <summary>
    /// Adds a command to the application.
    /// </summary>
    public CliApplicationBuilder AddCommand(CommandDescriptor commandDescriptor)
    {
        _commandDescriptors.Add(commandDescriptor);
        return this;
    }

    /// <summary>
    /// Adds multiple commands to the application.
    /// </summary>
    public CliApplicationBuilder AddCommands(IEnumerable<CommandDescriptor> commandDescriptors)
    {
        foreach (var commandDescriptor in commandDescriptors)
            AddCommand(commandDescriptor);

        return this;
    }

    /// <summary>
    /// Enables the debug mode, activated when the specified environment variable is set to <c>true</c>.
    /// When active, the application waits for a debugger to attach before executing the command.
    /// </summary>
    public CliApplicationBuilder AllowDebugMode(string? environmentVariableName)
    {
        _debugModeEnvironmentVariable = environmentVariableName;
        return this;
    }

    /// <summary>
    /// Enables the preview mode, activated when the specified environment variable is set to <c>true</c>.
    /// When active, the application prints the parsed command line before executing the command.
    /// </summary>
    public CliApplicationBuilder AllowPreviewMode(string? environmentVariableName)
    {
        _previewModeEnvironmentVariable = environmentVariableName;
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
    /// Configures the application to use the specified service provider for activating types.
    /// This method takes a delegate that receives the list of all added command descriptors, so that you can
    /// easily register their types with the service provider.
    /// </summary>
    public CliApplicationBuilder UseTypeActivator(
        Func<IReadOnlyList<CommandDescriptor>, IServiceProvider> getServiceProvider
    ) => UseTypeActivator(getServiceProvider([.. _commandDescriptors]));

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
            [.. _commandDescriptors],
            _debugModeEnvironmentVariable,
            _previewModeEnvironmentVariable
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

        return Path.GetFileNameWithoutExtension(processFilePath);
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
