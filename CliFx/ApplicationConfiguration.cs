namespace CliFx
{
    using System;
    using System.Collections.Generic;
    using CliFx.Exceptions;

    /// <summary>
    /// Configuration of the application.
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
        /// Initializes an instance of <see cref="ApplicationConfiguration"/>.
        /// </summary>
        public ApplicationConfiguration(IReadOnlyList<Type> commandTypes,
                                        IReadOnlyList<Type> customDirectives,
                                        ICliExceptionHandler exceptionHandler,
                                        bool isInteractiveModeAllowed)
        {
            CommandTypes = commandTypes;
            DirectiveTypes = customDirectives;
            ExceptionHandler = exceptionHandler;

            IsInteractiveModeAllowed = isInteractiveModeAllowed;
        }
    }
}