using System;
using System.Collections.Generic;
using CliFx.Exceptions;

namespace CliFx
{
    /// <summary>
    /// Configuration of an application.
    /// </summary>
    public class ApplicationConfiguration
    {
        /// <summary>
        /// Command types defined in this application.
        /// </summary>
        internal IReadOnlyList<Type> CommandTypes { get; }

        /// <summary>
        /// Custom directives defined in this application.
        /// </summary>
        internal IReadOnlyList<Type> DirectiveTypes { get; }

        /// <summary>
        /// Exception Handler instance.
        /// </summary>
        internal ICliExceptionHandler ExceptionHandler { get; }

        /// <summary>
        /// Whether interactive mode is allowed in this application.
        /// </summary>
        public bool IsInteractiveModeAllowed { get; }

        /// <summary>
        /// Command exit message level.
        /// </summary>
        public CommandExitMessageOptions CommandExitMessageOptions { get; }

        /// <summary>
        /// Command exit message foreground.
        /// </summary>
        public ConsoleColor CommandExitMessageForeground { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ApplicationConfiguration"/>.
        /// </summary>
        public ApplicationConfiguration(
            IReadOnlyList<Type> commandTypes,
            IReadOnlyList<Type> customDirectives,
            ICliExceptionHandler exceptionHandler,
            bool isInteractiveModeAllowed,
            CommandExitMessageOptions commandExitMessageOptions,
            ConsoleColor commandExitMessageForeground)
        {
            CommandTypes = commandTypes;
            DirectiveTypes = customDirectives;
            ExceptionHandler = exceptionHandler;

            IsInteractiveModeAllowed = isInteractiveModeAllowed;

            CommandExitMessageOptions = commandExitMessageOptions;
            CommandExitMessageForeground = commandExitMessageForeground;
        }
    }
}