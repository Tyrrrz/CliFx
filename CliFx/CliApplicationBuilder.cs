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
    public class CliApplicationBuilder : ICliApplicationBuilder
    {
        private readonly HashSet<Type> _commandTypes = new HashSet<Type>();

        private string _title;
        private string _executableName;
        private string _versionText;
        private IConsole _console;
        private ICommandFactory _commandFactory;

        public ICliApplicationBuilder WithCommand(Type commandType)
        {
            _commandTypes.Add(commandType);
            return this;
        }

        public ICliApplicationBuilder WithCommandsFrom(Assembly commandAssembly)
        {
            var commandTypes = commandAssembly.ExportedTypes.Where(t => t.Implements(typeof(ICommand)));

            foreach (var commandType in commandTypes)
                WithCommand(commandType);

            return this;
        }

        public ICliApplicationBuilder UseTitle(string title)
        {
            _title = title;
            return this;
        }

        public ICliApplicationBuilder UseExecutableName(string exeName)
        {
            _executableName = exeName;
            return this;
        }

        public ICliApplicationBuilder UseVersionText(string version)
        {
            _versionText = version;
            return this;
        }

        public ICliApplicationBuilder UseConsole(IConsole console)
        {
            _console = console;
            return this;
        }

        public ICliApplicationBuilder UseCommandFactory(ICommandFactory factory)
        {
            _commandFactory = factory;
            return this;
        }

        public ICliApplication Build()
        {
            // Entry assembly is null in tests
            var entryAssembly = Assembly.GetEntryAssembly();

            // Use defaults for required parameters that were not configured
            var title = _title ?? entryAssembly?.GetName().Name ?? "App";
            var executableName = _executableName ?? Path.GetFileNameWithoutExtension(entryAssembly?.Location) ?? "app";
            var versionText = _versionText ?? entryAssembly?.GetName().Version.ToString() ?? "1.0";
            var console = _console ?? new SystemConsole();
            var commandFactory = _commandFactory ?? new CommandFactory();

            // Project parameters to expected types
            var metadata = new ApplicationMetadata(title, executableName, versionText);
            var commandTypes = _commandTypes.ToArray();

            return new CliApplication(metadata, commandTypes,
                console, new CommandInputParser(), new CommandSchemaResolver(),
                commandFactory, new CommandInitializer(), new CommandHelpTextRenderer());
        }
    }
}