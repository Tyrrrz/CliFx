using System;
using System.Collections.Generic;
using System.Reflection;
using CliFx.Services;

namespace CliFx
{
    public static class Extensions
    {
        public static ICliApplicationBuilder WithCommands(this ICliApplicationBuilder builder, IReadOnlyList<Type> commandTypes)
        {
            foreach (var commandType in commandTypes)
                builder.WithCommand(commandType);

            return builder;
        }

        public static ICliApplicationBuilder WithCommandsFrom(this ICliApplicationBuilder builder, IReadOnlyList<Assembly> commandAssemblies)
        {
            foreach (var commandAssembly in commandAssemblies)
                builder.WithCommandsFrom(commandAssembly);

            return builder;
        }

        public static ICliApplicationBuilder WithCommandsFromThisAssembly(this ICliApplicationBuilder builder) =>
            builder.WithCommandsFrom(Assembly.GetCallingAssembly());

        public static ICliApplicationBuilder UseCommandFactory(this ICliApplicationBuilder builder, Func<Type, ICommand> factoryMethod) =>
            builder.UseCommandFactory(new DelegateCommandFactory(factoryMethod));
    }
}