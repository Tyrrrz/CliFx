using System;
using System.Collections.Generic;
using System.Reflection;
using CliFx.Services;

namespace CliFx
{
    /// <summary>
    /// Extensions for <see cref="CliFx"/>.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Adds multiple commands to the application.
        /// </summary>
        public static ICliApplicationBuilder WithCommands(this ICliApplicationBuilder builder, IReadOnlyList<Type> commandTypes)
        {
            foreach (var commandType in commandTypes)
                builder.WithCommand(commandType);

            return builder;
        }

        /// <summary>
        /// Adds commands from specified assemblies to the application.
        /// </summary>
        public static ICliApplicationBuilder WithCommandsFrom(this ICliApplicationBuilder builder, IReadOnlyList<Assembly> commandAssemblies)
        {
            foreach (var commandAssembly in commandAssemblies)
                builder.WithCommandsFrom(commandAssembly);

            return builder;
        }

        /// <summary>
        /// Adds commands from calling assembly to the application.
        /// </summary>
        public static ICliApplicationBuilder WithCommandsFromThisAssembly(this ICliApplicationBuilder builder) =>
            builder.WithCommandsFrom(Assembly.GetCallingAssembly());

        /// <summary>
        /// Configures application to use specified factory method for creating new instances of <see cref="ICommand"/>.
        /// </summary>
        public static ICliApplicationBuilder UseCommandFactory(this ICliApplicationBuilder builder, Func<Type, ICommand> factoryMethod) =>
            builder.UseCommandFactory(new DelegateCommandFactory(factoryMethod));
    }
}