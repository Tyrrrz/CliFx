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

        public static void WithForegroundColor(this IConsole console, ConsoleColor foregroundColor, Action action)
        {
            var lastColor = console.ForegroundColor;
            console.ForegroundColor = foregroundColor;

            action();

            console.ForegroundColor = lastColor;
        }

        public static void WithBackgroundColor(this IConsole console, ConsoleColor backgroundColor, Action action)
        {
            var lastColor = console.BackgroundColor;
            console.BackgroundColor = backgroundColor;

            action();

            console.BackgroundColor = lastColor;
        }

        public static void WithColors(this IConsole console, ConsoleColor foregroundColor, ConsoleColor backgroundColor, Action action) =>
            console.WithForegroundColor(foregroundColor, () => console.WithBackgroundColor(backgroundColor, action));
    }
}