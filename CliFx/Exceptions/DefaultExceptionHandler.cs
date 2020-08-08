using System;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Implementation of <see cref="ICliExceptionHandler"/> that prints all exceptions to console.
    /// </summary>
    public class DefaultExceptionHandler : ICliExceptionHandler
    {
        /// <summary>
        /// Initializes an instance of <see cref="DefaultExceptionHandler"/>.
        /// </summary>
        public DefaultExceptionHandler()
        {

        }

        /// <inheritdoc/>
        public void HandleCliFxException(IConsole console, CliFxException ex)
        {
            WriteError(console, ex.ToString());
            //ex.ShowHelp = false;
        }

        /// <inheritdoc/>
        public void HandleCommandException(IConsole console, CommandException ex)
        {
            WriteError(console, ex.ToString());
        }

        /// <inheritdoc/>
        public void HandleException(IConsole console, Exception ex)
        {
            WriteError(console, ex.ToString());
        }

        /// <summary>
        /// Write an error message to the console.
        /// </summary>
        protected static void WriteError(IConsole console, string message)
        {
            console.WithForegroundColor(ConsoleColor.Red, () => console.Error.WriteLine(message));
        }
    }
}
