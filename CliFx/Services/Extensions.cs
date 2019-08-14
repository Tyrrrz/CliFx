using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Extensions for <see cref="Services"/>
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Resolves command schemas for commands of specified types.
        /// </summary>
        public static IReadOnlyList<CommandSchema> GetCommandSchemas(this ICommandSchemaResolver resolver,
            IReadOnlyList<Type> commandTypes)
        {
            resolver.GuardNotNull(nameof(resolver));
            commandTypes.GuardNotNull(nameof(commandTypes));

            return commandTypes.Select(resolver.GetCommandSchema).ToArray();
        }

        /// <summary>
        /// Sets console foreground color, executes specified action, and sets the color back to the original value.
        /// </summary>
        public static void WithForegroundColor(this IConsole console, ConsoleColor foregroundColor, Action action)
        {
            console.GuardNotNull(nameof(console));
            action.GuardNotNull(nameof(action));

            var lastColor = console.ForegroundColor;
            console.ForegroundColor = foregroundColor;

            action();

            console.ForegroundColor = lastColor;
        }

        /// <summary>
        /// Sets console background color, executes specified action, and sets the color back to the original value.
        /// </summary>
        public static void WithBackgroundColor(this IConsole console, ConsoleColor backgroundColor, Action action)
        {
            console.GuardNotNull(nameof(console));
            action.GuardNotNull(nameof(action));

            var lastColor = console.BackgroundColor;
            console.BackgroundColor = backgroundColor;

            action();

            console.BackgroundColor = lastColor;
        }

        /// <summary>
        /// Sets console foreground and background colors, executes specified action, and sets the colors back to the original values.
        /// </summary>
        public static void WithColors(this IConsole console, ConsoleColor foregroundColor, ConsoleColor backgroundColor, Action action)
        {
            console.GuardNotNull(nameof(console));
            action.GuardNotNull(nameof(action));

            console.WithForegroundColor(foregroundColor, () => console.WithBackgroundColor(backgroundColor, action));
        }
    }
}