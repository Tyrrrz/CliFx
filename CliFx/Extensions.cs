﻿using System;

namespace CliFx
{
    /// <summary>
    /// Extensions for <see cref="CliFx"/>
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Sets console foreground color, executes specified action, and sets the color back to the original value.
        /// </summary>
        public static void WithForegroundColor(this IConsole console, ConsoleColor foregroundColor, Action action)
        {
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
            var lastColor = console.BackgroundColor;
            console.BackgroundColor = backgroundColor;

            action();

            console.BackgroundColor = lastColor;
        }

        /// <summary>
        /// Sets console foreground and background colors, executes specified action, and sets the colors back to the original values.
        /// </summary>
        public static void WithColors(this IConsole console, ConsoleColor foregroundColor, ConsoleColor backgroundColor, Action action) =>
            console.WithForegroundColor(foregroundColor, () => console.WithBackgroundColor(backgroundColor, action));
    }
}