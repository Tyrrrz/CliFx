using System;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Abstraction for exception handling.
    /// <remarks>
    /// Unfortunatelly, it is not possible to use constructor dependecy injection.
    /// </remarks>
    /// </summary>
    public interface ICliExceptionHandler
    {
        /// <summary>
        /// Handles exception of <see cref="CliFxException"/>.
        /// </summary>
        void HandleCliFxException(IConsole console, CliFxException ex);

        /// <summary>
        /// Handles exception of <see cref="CommandException"/>.
        /// </summary>
        void HandleCommandException(IConsole console, CommandException ex);

        /// <summary>
        /// Handles exception of <see cref="Exception"/>.
        /// </summary>
        void HandleException(IConsole console, Exception ex);
    }
}
