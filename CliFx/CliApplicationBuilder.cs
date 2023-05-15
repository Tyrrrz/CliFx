using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CliFx.Attributes;
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
    private readonly HashSet<Type> _commandTypes = new();

    private bool _isDebugModeAllowed = true;
    private bool _isPreviewModeAllowed = true;
    private string? _title;
    private string? _executableName;
    private string? _version;
    private string? _description;
    private IConsole? _console;
    private ITypeActivator? _typeActivator;

    /// <summary>
    /// Adds a command to the application.
    /// </summary>
    public CliApplicationBuilder AddCommand(Type commandType)
    {
        _commandTypes.Add(commandType);
        return this;
    }

    /// <summary>
    /// Adds a command to the application.
    /// </summary>
    public CliApplicationBuilder AddCommand<TCommand>() where TCommand : ICommand =>
        AddCommand(typeof(TCommand));

    /// <summary>
    /// Adds multiple commands to the application.
    /// </summary>
    public CliApplicationBuilder AddCommands(IEnumerable<Type> commandTypes)
    {
        foreach (var commandType in commandTypes)
            AddCommand(commandType);

        return this;
    }

    /// <summary>
    /// Adds commands from the specified assembly to the application.
    /// </summary>
    /// <remarks>
    /// This method looks for public non-abstract classes that implement <see cref="ICommand" />
    /// and are annotated by <see cref="CommandAttribute" />.
    /// </remarks>
    public CliApplicationBuilder AddCommandsFrom(Assembly commandAssembly)
    {
        foreach (var commandType in commandAssembly.ExportedTypes.Where(CommandSchema.IsCommandType))
            AddCommand(commandType);

        return this;
    }

    /// <summary>
    /// Adds commands from the specified assemblies to the application.
    /// </summary>
    /// <remarks>
    /// This method looks for public non-abstract classes that implement <see cref="ICommand" />
    /// and are annotated by <see cref="CommandAttribute" />.
    /// </remarks>
    public CliApplicationBuilder AddCommandsFrom(IEnumerable<Assembly> commandAssemblies)
    {
        foreach (var commandAssembly in commandAssemblies)
            AddCommandsFrom(commandAssembly);

        return this;
    }

    /// <summary>
    /// Adds commands from the calling assembly to the application.
    /// </summary>
    /// <remarks>
    /// This method looks for public non-abstract classes that implement <see cref="ICommand" />
    /// and are annotated by <see cref="CommandAttribute" />.
    /// </remarks>
    public CliApplicationBuilder AddCommandsFromThisAssembly() => AddCommandsFrom(Assembly.GetCallingAssembly());

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
        UseTypeActivator(serviceProvider.GetService);

    /// <summary>
    /// Configures the application to use the specified service provider for activating types.
    /// This method takes a delegate that receives the list of all added command types, so that you can
    /// easily register them with the service provider.
    /// </summary>
    public CliApplicationBuilder UseTypeActivator(Func<IReadOnlyList<Type>, IServiceProvider> getServiceProvider) =>
        UseTypeActivator(getServiceProvider(_commandTypes.ToArray()));

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
            _commandTypes.ToArray(),
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
                "Failed to infer the default application title. " +
                $"Please specify it explicitly using {nameof(SetTitle)}()."
            );
        }

        return entryAssemblyName;
    }

    private static string GetDefaultExecutableName()
    {
        var entryAssemblyFilePath = EnvironmentEx.EntryAssembly?.Location;
        var processFilePath = EnvironmentEx.ProcessPath;

        if (string.IsNullOrWhiteSpace(entryAssemblyFilePath) || string.IsNullOrWhiteSpace(processFilePath))
        {
            throw new InvalidOperationException(
                "Failed to infer the default application executable name. " +
                $"Please specify it explicitly using {nameof(SetExecutableName)}()."
            );
        }

        // If the process path matches the entry assembly path, it's a legacy .NET Framework app
        // or a self-contained .NET Core app.
        if (PathEx.AreEqual(entryAssemblyFilePath, processFilePath))
        {
            return Path.GetFileNameWithoutExtension(entryAssemblyFilePath);
        }

        // If the process path has the same name and parent directory as the entry assembly path,
        // but different extension, it's a framework-dependent .NET Core app launched through the apphost.
        if (PathEx.AreEqual(Path.ChangeExtension(entryAssemblyFilePath, "exe"), processFilePath) ||
            PathEx.AreEqual(Path.ChangeExtension(entryAssemblyFilePath, ""), processFilePath))
        {
            return Path.GetFileNameWithoutExtension(entryAssemblyFilePath);
        }

        // Otherwise, it's a framework-dependent .NET Core app launched through the .NET CLI
        return "dotnet " + Path.GetFileName(entryAssemblyFilePath);
    }

    private static string GetDefaultVersionText()
    {
        var entryAssemblyVersion = EnvironmentEx.EntryAssembly?.GetName().Version;
        if (entryAssemblyVersion is null)
        {
            throw new InvalidOperationException(
                "Failed to infer the default application version. " +
                $"Please specify it explicitly using {nameof(SetVersion)}()."
            );
        }

        return "v" + entryAssemblyVersion.ToSemanticString();
    }
}