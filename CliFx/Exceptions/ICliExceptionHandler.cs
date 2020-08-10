using System;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Abstraction for exception handling.
    /// </summary>
    public interface ICliExceptionHandler
    {
        /// <summary>
        /// Handles exception of <see cref="CliFxException"/>.
        /// </summary>
        void HandleCliFxException(IConsole console, ICliContext context, CliFxException ex);

        /// <summary>
        /// Handles exception of <see cref="CommandException"/>.
        /// </summary>
        void HandleCommandException(IConsole console, ICliContext context, CommandException ex);

        /// <summary>
        /// Handles exception of <see cref="Exception"/>.
        /// </summary>
        void HandleException(IConsole console, ICliContext context, Exception ex);
    }
}
