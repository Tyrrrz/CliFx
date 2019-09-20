using CliFx.Attributes;
using CliFx.Internal;
using CliFx.Models;
using CliFx.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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
        private string _title;
        private string _executableName;
        private string _versionText;
        private string _description;
        private IConsole _console;
        private ICommandFactory _commandFactory;
        private ICommandOptionInputConverter _commandOptionInputConverter;
        private IEnvironmentVariablesProvider _environmentVariablesProvider;

        /// <inheritdoc />
        public ICliApplicationBuilder AddCommand(Type commandType)
        {
            commandType.GuardNotNull(nameof(commandType));

            _commandTypes.Add(commandType);

            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder AddCommandsFrom(Assembly commandAssembly)
        {
            commandAssembly.GuardNotNull(nameof(commandAssembly));

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
            _title = title.GuardNotNull(nameof(title));
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseExecutableName(string executableName)
        {
            _executableName = executableName.GuardNotNull(nameof(executableName));
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseVersionText(string versionText)
        {
            _versionText = versionText.GuardNotNull(nameof(versionText));
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseDescription(string description)
        {
            _description = description; // can be null
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseConsole(IConsole console)
        {
            _console = console.GuardNotNull(nameof(console));
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseCommandFactory(ICommandFactory factory)
        {
            _commandFactory = factory.GuardNotNull(nameof(factory));
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseCommandOptionInputConverter(ICommandOptionInputConverter converter)
        {
            _commandOptionInputConverter = converter.GuardNotNull(nameof(converter));
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseEnvironmentVariablesProvider(IEnvironmentVariablesProvider environmentVariablesProvider)
        {
            _environmentVariablesProvider = environmentVariablesProvider.GuardNotNull(nameof(environmentVariablesProvider));
            return this;
        }

        /// <inheritdoc />
        public ICliApplication Build()
        {
            // Use defaults for required parameters that were not configured
            _title = _title ?? GetDefaultTitle() ?? "App";
            _executableName = _executableName ?? GetDefaultExecutableName() ?? "app";
            _versionText = _versionText ?? GetDefaultVersionText() ?? "v1.0";
            _console = _console ?? new SystemConsole();
            _commandFactory = _commandFactory ?? new CommandFactory();
            _commandOptionInputConverter = _commandOptionInputConverter ?? new CommandOptionInputConverter();
            _environmentVariablesProvider = _environmentVariablesProvider ?? new EnvironmentVariablesProvider();

            // Project parameters to expected types
            var metadata = new ApplicationMetadata(_title, _executableName, _versionText, _description);
            var configuration = new ApplicationConfiguration(_commandTypes.ToArray(), _isDebugModeAllowed, _isPreviewModeAllowed);

            return new CliApplication(metadata, configuration,
                _console, new CommandInputParser(), new CommandSchemaResolver(),
                _commandFactory, new CommandInitializer(_commandOptionInputConverter, _environmentVariablesProvider), new HelpTextRenderer());
        }
    }

    public partial class CliApplicationBuilder
    {
        private static readonly Lazy<Assembly> LazyEntryAssembly = new Lazy<Assembly>(Assembly.GetEntryAssembly);

        // Entry assembly is null in tests
        private static Assembly EntryAssembly => LazyEntryAssembly.Value;

        private static string GetDefaultTitle() => EntryAssembly?.GetName().Name;

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

        private static string GetDefaultVersionText() => EntryAssembly != null ? $"v{EntryAssembly.GetName().Version}" : null;
    }
}