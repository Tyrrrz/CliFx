using System;
using System.Collections.Generic;
using System.Reflection;
using CliFx.Models;
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
        public static ICliApplicationBuilder AddCommands(this ICliApplicationBuilder builder, IReadOnlyList<Type> commandTypes)
        {
            foreach (var commandType in commandTypes)
                builder.AddCommand(commandType);

            return builder;
        }

        /// <summary>
        /// Adds commands from specified assemblies to the application.
        /// </summary>
        public static ICliApplicationBuilder AddCommandsFrom(this ICliApplicationBuilder builder, IReadOnlyList<Assembly> commandAssemblies)
        {
            foreach (var commandAssembly in commandAssemblies)
                builder.AddCommandsFrom(commandAssembly);

            return builder;
        }

        /// <summary>
        /// Adds commands from calling assembly to the application.
        /// </summary>
        public static ICliApplicationBuilder AddCommandsFromThisAssembly(this ICliApplicationBuilder builder) =>
            builder.AddCommandsFrom(Assembly.GetCallingAssembly());

        /// <summary>
        /// Configures application to use specified factory method for creating new instances of <see cref="ICommand"/>.
        /// </summary>
        public static ICliApplicationBuilder UseCommandFactory(this ICliApplicationBuilder builder, Func<ICommandSchema, ICommand> factoryMethod) =>
            builder.UseCommandFactory(new DelegateCommandFactory(factoryMethod));
    }
}