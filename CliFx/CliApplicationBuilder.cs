using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CliFx.Attributes;
using CliFx.Internal;
using CliFx.Models;
using CliFx.Services;

namespace CliFx
{
    /// <summary>
    /// Default implementation of <see cref="ICliApplicationBuilder"/>.
    /// </summary>
    public partial class CliApplicationBuilder : ICliApplicationBuilder
    {
        private readonly HashSet<Type> _commandTypes = new HashSet<Type>();

        private bool _isDebugModeAllowed = true;
        private bool _isPreviewModeAllowed = true;
        private string? _title;
        private string? _executableName;
        private string? _versionText;
        private string? _description;
        private IConsole? _console;
        private ICommandFactory? _commandFactory;
        private ICommandInputConverter? _commandInputConverter;
        private IEnvironmentVariablesProvider? _environmentVariablesProvider;

        /// <inheritdoc />
        public ICliApplicationBuilder AddCommand(Type commandType)
        {
            _commandTypes.Add(commandType);

            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder AddCommandsFrom(Assembly commandAssembly)
        {
            var commandTypes = commandAssembly.ExportedTypes
                .Where(t => t.Implements(typeof(ICommand)))
                .Where(t => t.IsDefined(typeof(CommandAttribute)))
                .Where(t => !t.IsAbstract && !t.IsInterface);

            foreach (var commandType in commandTypes)
                AddCommand(commandType);

            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder AllowDebugMode(bool isAllowed = true)
        {
            _isDebugModeAllowed = isAllowed;
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder AllowPreviewMode(bool isAllowed = true)
        {
            _isPreviewModeAllowed = isAllowed;
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseTitle(string title)
        {
            _title = title;
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseExecutableName(string executableName)
        {
            _executableName = executableName;
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseVersionText(string versionText)
        {
            _versionText = versionText;
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseDescription(string? description)
        {
            _description = description;
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseConsole(IConsole console)
        {
            _console = console;
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseCommandFactory(ICommandFactory factory)
        {
            _commandFactory = factory;
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseCommandOptionInputConverter(ICommandInputConverter converter)
        {
            _commandInputConverter = converter;
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseEnvironmentVariablesProvider(IEnvironmentVariablesProvider environmentVariablesProvider)
        {
            _environmentVariablesProvider = environmentVariablesProvider;
            return this;
        }

        /// <inheritdoc />
        public ICliApplication Build()
        {
            // Use defaults for required parameters that were not configured
            _title ??= GetDefaultTitle() ?? "App";
            _executableName ??= GetDefaultExecutableName() ?? "app";
            _versionText ??= GetDefaultVersionText() ?? "v1.0";
            _console ??= new SystemConsole();
            _commandFactory ??= new CommandFactory();
            _commandInputConverter ??= new CommandInputConverter();
            _environmentVariablesProvider ??= new EnvironmentVariablesProvider();

            // Project parameters to expected types
            var metadata = new ApplicationMetadata(_title, _executableName, _versionText, _description);
            var configuration = new ApplicationConfiguration(_commandTypes.ToArray(), _isDebugModeAllowed, _isPreviewModeAllowed);

            return new CliApplication(metadata, configuration,
                _console, new CommandInputParser(_environmentVariablesProvider), new CommandSchemaResolver(new CommandArgumentSchemasValidator()),
                _commandFactory, new CommandInitializer(_commandInputConverter, new EnvironmentVariablesParser()), new HelpTextRenderer());
        }
    }

    public partial class CliApplicationBuilder
    {
        private static readonly Lazy<Assembly> LazyEntryAssembly = new Lazy<Assembly>(Assembly.GetEntryAssembly);

        // Entry assembly is null in tests
        private static Assembly EntryAssembly => LazyEntryAssembly.Value;

        private static string GetDefaultTitle() => EntryAssembly?.GetName().Name ?? "";

        private static string GetDefaultExecutableName()
        {
            var entryAssemblyLocation = EntryAssembly?.Location;

            // If it's a .dll assembly, prepend 'dotnet' and keep the file extension
            if (string.Equals(Path.GetExtension(entryAssemblyLocation), ".dll", StringComparison.OrdinalIgnoreCase))
            {
                return "dotnet " + Path.GetFileName(entryAssemblyLocation);
            }

            // Otherwise just use assembly file name without extension
            return Path.GetFileNameWithoutExtension(entryAssemblyLocation);
        }

        private static string GetDefaultVersionText() => EntryAssembly != null ? $"v{EntryAssembly.GetName().Version}" : "";
    }
}