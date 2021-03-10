﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CliFx.Attributes;
using CliFx.Infrastructure;
using CliFx.Schema;
using CliFx.Utils.Extensions;

namespace CliFx
{
    /// <summary>
    /// Builds an instance of <see cref="CliApplication"/>.
    /// </summary>
    public partial class CliApplicationBuilder
    {
        private readonly HashSet<Type> _commandTypes = new();

        private bool _isDebugModeAllowed = true;
        private bool _isPreviewModeAllowed = true;
        private string? _title;
        private string? _executableName;
        private string? _versionText;
        private string? _description;
        private IConsole? _console;
        private ITypeActivator? _typeActivator;

        /// <summary>
        /// Adds a command of specified type to the application.
        /// </summary>
        public CliApplicationBuilder AddCommand(Type commandType)
        {
            _commandTypes.Add(commandType);

            return this;
        }

        /// <summary>
        /// Adds a command of specified type to the application.
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
        ///
        /// Only adds public non-abstract types that implement <see cref="ICommand"/>
        /// and are annotated by <see cref="CommandAttribute"/>.
        /// </summary>
        public CliApplicationBuilder AddCommandsFrom(Assembly commandAssembly)
        {
            foreach (var commandType in commandAssembly.ExportedTypes.Where(CommandSchema.IsCommandType))
                AddCommand(commandType);

            return this;
        }

        /// <summary>
        /// Adds commands from the specified assemblies to the application.
        ///
        /// Only adds public non-abstract types that implement <see cref="ICommand"/>
        /// and are annotated by <see cref="CommandAttribute"/>.
        /// </summary>
        public CliApplicationBuilder AddCommandsFrom(IEnumerable<Assembly> commandAssemblies)
        {
            foreach (var commandAssembly in commandAssemblies)
                AddCommandsFrom(commandAssembly);

            return this;
        }

        /// <summary>
        /// Adds commands from the calling assembly to the application.
        ///
        /// Only adds public non-abstract types that implement <see cref="ICommand"/>
        /// and are annotated by <see cref="CommandAttribute"/>.
        /// </summary>
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
        /// Sets application title, which appears in the help text.
        /// </summary>
        public CliApplicationBuilder SetTitle(string title)
        {
            _title = title;
            return this;
        }

        /// <summary>
        /// Sets application executable name, which appears in the help text.
        /// </summary>
        public CliApplicationBuilder SetExecutableName(string executableName)
        {
            _executableName = executableName;
            return this;
        }

        /// <summary>
        /// Sets application version text, which appears in the help text and when the user requests version information.
        /// </summary>
        public CliApplicationBuilder SetVersion(string version)
        {
            _versionText = version;
            return this;
        }

        /// <summary>
        /// Sets application description, which appears in the help text.
        /// </summary>
        public CliApplicationBuilder SetDescription(string? description)
        {
            _description = description;
            return this;
        }

        /// <summary>
        /// Configures the application to use the specified implementation of <see cref="IConsole"/>.
        /// </summary>
        public CliApplicationBuilder UseConsole(IConsole console)
        {
            _console = console;
            return this;
        }

        /// <summary>
        /// Configures the application to use the specified implementation of <see cref="ITypeActivator"/>.
        /// </summary>
        public CliApplicationBuilder UseTypeActivator(ITypeActivator typeActivator)
        {
            _typeActivator = typeActivator;
            return this;
        }

        /// <summary>
        /// Configures the application to use the specified function for activating types.
        /// </summary>
        public CliApplicationBuilder UseTypeActivator(Func<Type, object> typeActivator) =>
            UseTypeActivator(new DelegateTypeActivator(typeActivator));

        /// <summary>
        /// Creates an instance of <see cref="CliApplication"/> using configured parameters.
        /// Default values are used in place of parameters that were not specified.
        /// </summary>
        public CliApplication Build()
        {
            _title ??= GetDefaultTitle();
            _executableName ??= GetDefaultExecutableName();
            _versionText ??= GetDefaultVersionText();
            _console ??= new SystemConsole();
            _typeActivator ??= new DefaultTypeActivator();

            var metadata = new ApplicationMetadata(_title, _executableName, _versionText, _description);
            var configuration = new ApplicationConfiguration(_commandTypes.ToArray(), _isDebugModeAllowed, _isPreviewModeAllowed);

            return new CliApplication(metadata, configuration, _console, _typeActivator);
        }
    }

    public partial class CliApplicationBuilder
    {
        private static readonly Lazy<Assembly?> LazyEntryAssembly = new(Assembly.GetEntryAssembly);

        // Entry assembly is null in tests
        private static Assembly? EntryAssembly => LazyEntryAssembly.Value;

        private static string GetDefaultTitle() => EntryAssembly?.GetName().Name?? "App";

        private static string GetDefaultExecutableName()
        {
            var entryAssemblyLocation = EntryAssembly?.Location;

            // The assembly can be an executable or a dll, depending on how it was packaged
            var isDll = string.Equals(
                Path.GetExtension(entryAssemblyLocation),
                ".dll",
                StringComparison.OrdinalIgnoreCase
            );

            var name = isDll
                ? "dotnet " + Path.GetFileName(entryAssemblyLocation)
                : Path.GetFileNameWithoutExtension(entryAssemblyLocation);

            return name ?? "app";
        }

        private static string GetDefaultVersionText() =>
            EntryAssembly is not null
                ? $"v{EntryAssembly.GetName().Version.ToSemanticString()}"
                : "v1.0";
    }
}