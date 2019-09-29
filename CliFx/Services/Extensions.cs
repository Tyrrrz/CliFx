using System;
using System.Collections.Generic;
using CliFx.Internal;

namespace CliFx.Services
{
    /// <summary>
    /// Extensions for <see cref="Services"/>
    /// </summary>
    public static class Extensions
    {
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

        /// <summary>
        /// Gets wether a string representing an environment variable value is escaped (i.e.: surrounded by double quotation marks)
        /// </summary>
        public static bool IsEnvironmentVariableEscaped(this string environmentVariableValue)
        {
            environmentVariableValue.GuardNotNull(nameof(environmentVariableValue));

            return environmentVariableValue.StartsWith("\"") && environmentVariableValue.EndsWith("\"");
        }

        /// <summary>
        /// Gets wether the <see cref="Type"/> supplied is a collection implementing <see cref="IEnumerable{T}"/>
        /// </summary>
        public static bool IsCollection(this Type type)
        {
            type.GuardNotNull(nameof(type));

            return type != typeof(string) && type.GetEnumerableUnderlyingType() != null;
        }
    }
}