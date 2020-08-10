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
        public void HandleCliFxException(IConsole console, ICliContext context, CliFxException ex)
        {
            WriteError(console, ex.ToString());
            //ex.ShowHelp = false;
        }

        /// <inheritdoc/>
        public void HandleCommandException(IConsole console, ICliContext context, CommandException ex)
        {
            WriteError(console, ex.ToString());
        }

        /// <inheritdoc/>
        public void HandleException(IConsole console, ICliContext context, Exception ex)
        {
            if (context.CurrentCommand is null)
            {
                console.WithForegroundColor(ConsoleColor.Red, () =>
                    console.Error.WriteLine($"Fatal error occured in {context.Metadata.ExecutableName}."));
            }
            else
            {
                console.WithForegroundColor(ConsoleColor.Red, () =>
                    console.Error.WriteLine($"Fatal error occured in {context.Metadata.ExecutableName} during execution of '{context.CurrentCommand.Name ?? "default"}' command."));
            }

            console.Error.WriteLine();
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
