using System;
using System.Reflection;
using CliFx.Services;

namespace CliFx
{
    /// <summary>
    /// Builds an instance of <see cref="ICliApplication"/>.
    /// </summary>
    public interface ICliApplicationBuilder
    {
        /// <summary>
        /// Adds a command of specified type to the application.
        /// </summary>
        ICliApplicationBuilder AddCommand(Type commandType);

        /// <summary>
        /// Adds commands from specified assembly to the application.
        /// </summary>
        ICliApplicationBuilder AddCommandsFrom(Assembly commandAssembly);

        /// <summary>
        /// Specifies whether debug mode (enabled with [debug] directive) is allowed in the application.
        /// </summary>
        ICliApplicationBuilder AllowDebugMode(bool isAllowed = true);

        /// <summary>
        /// Specifies whether preview mode (enabled with [preview] directive) is allowed in the application.
        /// </summary>
        ICliApplicationBuilder AllowPreviewMode(bool isAllowed = true);

        /// <summary>
        /// Sets application title, which appears in the help text.
        /// </summary>
        ICliApplicationBuilder UseTitle(string title);

        /// <summary>
        /// Sets application executable name, which appears in the help text.
        /// </summary>
        ICliApplicationBuilder UseExecutableName(string executableName);

        /// <summary>
        /// Sets application version text, which appears in the help text and when the user requests version information.
        /// </summary>
        ICliApplicationBuilder UseVersionText(string versionText);

        /// <summary>
        /// Sets application description, which appears in the help text.
        /// </summary>
        ICliApplicationBuilder UseDescription(string? description);

        /// <summary>
        /// Configures application to use specified implementation of <see cref="IConsole"/>.
        /// </summary>
        ICliApplicationBuilder UseConsole(IConsole console);

        /// <summary>
        /// Configures application to use specified implementation of <see cref="ICommandFactory"/>.
        /// </summary>
        ICliApplicationBuilder UseCommandFactory(ICommandFactory factory);

        /// <summary>
        /// Configures application to use specified implementation of <see cref="ICommandInputConverter"/>.
        /// </summary>
        ICliApplicationBuilder UseCommandOptionInputConverter(ICommandInputConverter converter);

        /// <summary>
        /// Configures application to use specified implementation of <see cref="IEnvironmentVariablesProvider"/>.
        /// </summary>
        ICliApplicationBuilder UseEnvironmentVariablesProvider(IEnvironmentVariablesProvider environmentVariablesProvider);

        /// <summary>
        /// Creates an instance of <see cref="ICliApplication"/> using configured parameters.
        /// Default values are used in place of parameters that were not specified.
        /// </summary>
        ICliApplication Build();
    }
}