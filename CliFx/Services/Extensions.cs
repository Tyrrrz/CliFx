using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Models;

namespace CliFx.Services
{
    public static class Extensions
    {
        public static IReadOnlyList<CommandSchema> GetCommandSchemas(this ICommandSchemaResolver resolver,
            IReadOnlyList<Type> commandTypes) =>
            commandTypes.Select(resolver.GetCommandSchema).ToArray();

        public static void WithColor(this IConsole console, ConsoleColor foregroundColor, Action action)
        {
            var lastForegroundColor = console.ForegroundColor;
            console.ForegroundColor = foregroundColor;

            action();

            console.ForegroundColor = lastForegroundColor;
        }

        public static void WithColor(this IConsole console, ConsoleColor foregroundColor, ConsoleColor backgroundColor, Action action)
        {
            var lastBackgroundColor = console.BackgroundColor;
            console.BackgroundColor = backgroundColor;

            console.WithColor(foregroundColor, action);

            console.BackgroundColor = lastBackgroundColor;
        }
    }
}