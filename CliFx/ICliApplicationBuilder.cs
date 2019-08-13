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
        ICliApplicationBuilder WithCommand(Type commandType);

        /// <summary>
        /// Adds commands from specified assembly to the application.
        /// </summary>
        ICliApplicationBuilder WithCommandsFrom(Assembly commandAssembly);

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
        ICliApplicationBuilder UseVersionText(string version);

        /// <summary>
        /// Configures application to use specified implementation of <see cref="IConsole"/>.
        /// </summary>
        ICliApplicationBuilder UseConsole(IConsole console);

        /// <summary>
        /// Configures application to use specified implementation of <see cref="ICommandFactory"/>.
        /// </summary>
        ICliApplicationBuilder UseCommandFactory(ICommandFactory factory);

        /// <summary>
        /// Creates an instance of <see cref="ICliApplication"/> using configured parameters.
        /// Default values are used in place of parameters that were not specified.
        /// </summary>
        ICliApplication Build();
    }
}