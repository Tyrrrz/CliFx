using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        private string _title;
        private string _executableName;
        private string _versionText;
        private string _description;
        private IConsole _console;
        private ICommandFactory _commandFactory;

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

            var commandTypes = commandAssembly.ExportedTypes.Where(t => t.Implements(typeof(ICommand)));

            foreach (var commandType in commandTypes)
                AddCommand(commandType);

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

        private void SetFallbackValues()
        {
            if (_title.IsNullOrWhiteSpace())
            {
                // Entry assembly is null in tests
                var title = EntryAssembly?.GetName().Name ?? "App";

                UseTitle(title);
            }

            if (_executableName.IsNullOrWhiteSpace())
            {
                // Entry assembly is null in tests
                var entryAssemblyLocation = EntryAssembly?.Location;
                var executableName = Path.GetFileNameWithoutExtension(entryAssemblyLocation) ?? "app";

                // Set proper executable name for apps launched with dotnet SDK
                if (string.Equals(Path.GetExtension(entryAssemblyLocation), ".dll", StringComparison.OrdinalIgnoreCase))
                    executableName = "dotnet " + executableName;

                UseExecutableName(executableName);
            }

            if (_versionText.IsNullOrWhiteSpace())
            {
                // Entry assembly is null in tests
                var versionText = EntryAssembly?.GetName().Version.ToString() ?? "1.0";

                UseVersionText(versionText);
            }

            if (_console == null)
            {
                UseConsole(new SystemConsole());
            }

            if (_commandFactory == null)
            {
                UseCommandFactory(new CommandFactory());
            }
        }

        /// <inheritdoc />
        public ICliApplication Build()
        {
            // Use defaults for required parameters that were not configured
            SetFallbackValues();

            // Project parameters to expected types
            var metadata = new ApplicationMetadata(_title, _executableName, _versionText, _description);
            var configuration = new ApplicationConfiguration(_commandTypes.ToArray());

            return new CliApplication(metadata, configuration,
                _console, new CommandInputParser(), new CommandSchemaResolver(),
                _commandFactory, new CommandInitializer(), new HelpTextRenderer());
        }
    }

    public partial class CliApplicationBuilder
    {
        private static readonly Lazy<Assembly> LazyEntryAssembly = new Lazy<Assembly>(Assembly.GetEntryAssembly);

        private static Assembly EntryAssembly => LazyEntryAssembly.Value;
    }
}