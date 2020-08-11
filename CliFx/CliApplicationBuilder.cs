using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CliFx.Domain;
using CliFx.Exceptions;
using CliFx.Internal.Extensions;

namespace CliFx
{
    /// <summary>
    /// Builds an instance of <see cref="CliApplication"/>.
    /// </summary>
    public partial class CliApplicationBuilder
    {
        //Directives and commands settings
        private readonly List<Type> _commandTypes = new List<Type>();
        private readonly List<Type> _customDirectives = new List<Type>();

        //Metadata settings
        private string? _title;
        private string? _executableName;
        private string? _versionText;
        private string? _description;
        private string? _startupMessage;

        //Exceptions
        private ICliExceptionHandler? _exceptionHandler;
        private CommandExitMessageOptions _commandExitMessageLevel;
        private ConsoleColor _exitMessageForeground = ConsoleColor.White;

        // Dependecy injection and type activation
        private IConsole? _console;
        private ITypeActivator? _typeActivator;
        private Func<ICliContext, IConsole, Func<Type, object>>? _buildServiceProvider;

        //Interactive mode settings
        private bool _isInteractiveModeAllowed = false;
        private ConsoleColor _promptForeground = ConsoleColor.Blue;
        private ConsoleColor _commandForeground = ConsoleColor.Yellow;

        #region Directives
        /// <summary>
        /// Add a custom directive to the application.
        /// </summary>
        public CliApplicationBuilder AddDirective(Type directive)
        {
            _customDirectives.Add(directive);

            return this;
        }

        /// <summary>
        /// Add a custom directive to the application.
        /// </summary>
        public CliApplicationBuilder AddDirective<T>()
            where T : IDirective
        {
            _customDirectives.Add(typeof(T));

            return this;
        }

        /// <summary>
        /// Add custom directives to the application.
        /// </summary>
        public CliApplicationBuilder AddDirectives(IEnumerable<Type> directiveTypes)
        {
            foreach (var directiveType in directiveTypes)
                AddDirective(directiveType);

            return this;
        }

        /// <summary>
        /// Adds directives from the specified assembly to the application.
        /// Only adds public valid directive types.
        /// </summary>
        public CliApplicationBuilder AddDirectivesFrom(Assembly directiveAssembly)
        {
            foreach (var directiveType in directiveAssembly.ExportedTypes.Where(CommandSchema.IsCommandType))
                AddCommand(directiveType);

            return this;
        }

        /// <summary>
        /// Adds directives from the specified assemblies to the application.
        /// Only adds public valid directive types.
        /// </summary>
        public CliApplicationBuilder AddDirectivesFrom(IEnumerable<Assembly> directiveAssemblies)
        {
            foreach (var directiveType in directiveAssemblies)
                AddCommandsFrom(directiveType);

            return this;
        }

        /// <summary>
        /// Adds directives from the calling assembly to the application.
        /// Only adds public valid directive types.
        /// </summary>
        public CliApplicationBuilder AddDirectivesFromThisAssembly()
        {
            return AddDirectivesFrom(Assembly.GetCallingAssembly());
        }
        #endregion

        #region Commands
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
        public CliApplicationBuilder AddCommand<T>()
            where T : ICommand
        {
            _commandTypes.Add(typeof(T));

            return this;
        }

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
        /// Only adds public valid command types.
        /// </summary>
        public CliApplicationBuilder AddCommandsFrom(Assembly commandAssembly)
        {
            foreach (var commandType in commandAssembly.ExportedTypes.Where(CommandSchema.IsCommandType))
                AddCommand(commandType);

            return this;
        }

        /// <summary>
        /// Adds commands from the specified assemblies to the application.
        /// Only adds public valid command types.
        /// </summary>
        public CliApplicationBuilder AddCommandsFrom(IEnumerable<Assembly> commandAssemblies)
        {
            foreach (var commandAssembly in commandAssemblies)
                AddCommandsFrom(commandAssembly);

            return this;
        }

        /// <summary>
        /// Adds commands from the calling assembly to the application.
        /// Only adds public valid command types.
        /// </summary>
        public CliApplicationBuilder AddCommandsFromThisAssembly()
        {
            return AddCommandsFrom(Assembly.GetCallingAssembly());
        }
        #endregion

        #region Metadata
        /// <summary>
        /// Sets application title, which appears in the help text.
        /// </summary>
        public CliApplicationBuilder UseTitle(string title)
        {
            _title = title;
            return this;
        }

        /// <summary>
        /// Sets application executable name, which appears in the help text.
        /// </summary>
        public CliApplicationBuilder UseExecutableName(string executableName)
        {
            _executableName = executableName;
            return this;
        }

        /// <summary>
        /// Sets application version text, which appears in the help text and when the user requests version information.
        /// </summary>
        public CliApplicationBuilder UseVersionText(string versionText)
        {
            _versionText = versionText;
            return this;
        }

        /// <summary>
        /// Sets application description, which appears in the help text.
        /// </summary>
        public CliApplicationBuilder UseDescription(string? description)
        {
            _description = description;
            return this;
        }

        /// <summary>
        /// Sets application startup message, which appears just after starting the app.
        ///
        /// You can use the following macros:
        ///     `{title}` for application title,
        ///     `{executable}` for executable name,
        ///     `{version}` for application version,
        ///     `{description}` for application description.
        ///
        /// Double braces can be used to escape macro replacement, while unknown macros will simply act as if they were escaped.
        /// </summary>
        public CliApplicationBuilder UseStartupMessage(string? message)
        {
            _startupMessage = message;
            return this;
        }
        #endregion

        #region Dependecy injection and type activation
        /// <summary>
        /// Configures the application to use the specified implementation of <see cref="IConsole"/>.
        /// </summary>
        public CliApplicationBuilder UseConsole(IConsole console)
        {
            _console = console;
            return this;
        }

        /// <summary>
        /// Configures the application to use the specified implementation of <see cref="IConsole"/>.
        /// </summary>
        public CliApplicationBuilder UseConsole<T>() where T : class, IConsole, new()
        {
            _console = new T();
            return this;
        }

        /// <summary>
        /// Configures the application to use the specified implementation of <see cref="ITypeActivator"/>.
        /// </summary>
        public CliApplicationBuilder UseTypeActivator(ITypeActivator typeActivator)
        {
            _buildServiceProvider = null;
            _typeActivator = typeActivator;

            return this;
        }

        /// <summary>
        /// Configures the application to use the specified function for activating types.
        /// </summary>
        public CliApplicationBuilder UseTypeActivator(Func<Type, object> typeActivator)
        {
            return UseTypeActivator(new DelegateTypeActivator(typeActivator));
        }

        /// <summary>
        /// Configures the application to use the specified implementation of <see cref="ITypeActivator"/>.
        /// </summary>
        public CliApplicationBuilder UseTypeActivator(Func<ICliContext, IConsole, Func<Type, object>> buildServiceProvider)
        {
            //TODO: consider using Microsoft.Extensions.DependencyInjection

            _typeActivator = null;
            _buildServiceProvider = buildServiceProvider;

            return this;
        }
        #endregion

        #region Exceptions
        /// <summary>
        /// Configures the exit code reporting level.
        /// </summary>
        public CliApplicationBuilder UseCommandExitMessageLevel(CommandExitMessageOptions option)
        {
            _commandExitMessageLevel = option;
            return this;
        }

        /// <summary>
        /// Configures command exit message foreground color.
        /// </summary>
        public CliApplicationBuilder UseCommandExitMessageForeground(ConsoleColor color)
        {
            _exitMessageForeground = color;
            return this;
        }

        /// <summary>
        /// Configures the application to use the specified implementation of <see cref="ICliExceptionHandler"/>.
        /// </summary>
        public CliApplicationBuilder UseExceptionHandler<T>()
            where T : class, ICliExceptionHandler, new()
        {
            _exceptionHandler = new T();
            return this;
        }

        /// <summary>
        /// Configures the application to use the specified implementation of <see cref="ICliExceptionHandler"/>.
        /// </summary>
        public CliApplicationBuilder UseExceptionHandler(ICliExceptionHandler handler)
        {
            _exceptionHandler = handler;
            return this;
        }
        #endregion

        #region Interactive Mode

        /// <summary>
        /// Configures whether interactive mode (enabled with [interactive] directive) is allowed in the application.
        /// </summary>
        public CliApplicationBuilder AllowInteractiveMode(bool isAllowed = true)
        {
            _isInteractiveModeAllowed = isAllowed;
            return this;
        }

        /// <summary>
        /// Configures the command prompt foreground color in interactive mode.
        /// </summary>
        public CliApplicationBuilder UsePromptForeground(ConsoleColor color)
        {
            _promptForeground = color;
            return this;
        }

        /// <summary>
        /// Configures the command input foreground color in interactive mode.
        /// </summary>
        public CliApplicationBuilder UseCommandInputForeground(ConsoleColor color)
        {
            _commandForeground = color;
            return this;
        }
        #endregion

        /// <summary>
        /// Creates an instance of <see cref="CliApplication"/> or <see cref="InteractiveCliApplication"/> using configured parameters.
        /// Default values are used in place of parameters that were not specified.
        /// </summary>
        public CliApplication Build()
        {
            _title ??= TryGetDefaultTitle() ?? "App";
            _executableName ??= TryGetDefaultExecutableName() ?? "app";
            _versionText ??= TryGetDefaultVersionText() ?? "v1.0";
            _console ??= new SystemConsole();
            _exceptionHandler ??= new DefaultExceptionHandler();

            if (_startupMessage != null)
            {
                _startupMessage = Regex.Replace(_startupMessage, @"{(?<x>[^}]+)}", match =>
                {
                    string value = match.Groups["x"].Value;

                    return value.ToLower() switch
                    {
                        "title" => _title,
                        "executable" => _executableName,
                        "version" => _versionText,
                        "description" => _description ?? string.Empty,
                        _ => string.Concat("{", value, "}")
                    };
                });
            }

            var metadata = new ApplicationMetadata(_title, _executableName, _versionText, _description, _startupMessage);
            var configuration = new ApplicationConfiguration(_commandTypes,
                                                             _customDirectives,
                                                             _exceptionHandler,
                                                             _isInteractiveModeAllowed,
                                                             _commandExitMessageLevel,
                                                             _exitMessageForeground);

            CliContext cliContext = new CliContext(metadata, configuration, _console);

            if (_buildServiceProvider is null)
                _typeActivator ??= new DefaultTypeActivator();
            else
                _typeActivator = new DelegateTypeActivator(_buildServiceProvider.Invoke(cliContext, _console));

            if (_isInteractiveModeAllowed)
            {
                return new InteractiveCliApplication(cliContext,
                                                     _typeActivator,
                                                     _promptForeground,
                                                     _commandForeground);
            }

            return new CliApplication(cliContext, _typeActivator);
        }
    }

    public partial class CliApplicationBuilder
    {
        private static readonly Lazy<Assembly?> LazyEntryAssembly = new Lazy<Assembly?>(Assembly.GetEntryAssembly);

        // Entry assembly is null in tests
        private static Assembly? EntryAssembly => LazyEntryAssembly.Value;

        private static string? TryGetDefaultTitle()
        {
            return EntryAssembly?.GetName().Name;
        }

        private static string? TryGetDefaultExecutableName()
        {
            string? entryAssemblyLocation = EntryAssembly?.Location;

            // The assembly can be an executable or a dll, depending on how it was packaged
            bool isDll = string.Equals(Path.GetExtension(entryAssemblyLocation), ".dll", StringComparison.OrdinalIgnoreCase);

            return isDll
                ? "dotnet " + Path.GetFileName(entryAssemblyLocation)
                : Path.GetFileNameWithoutExtension(entryAssemblyLocation);
        }

        private static string? TryGetDefaultVersionText()
        {
            return EntryAssembly != null ? $"v{EntryAssembly.GetName().Version.ToSemanticString()}" : null;
        }
    }
}