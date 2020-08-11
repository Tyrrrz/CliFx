using System;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Implementation of <see cref="ICliExceptionHandler"/> that prints all exceptions to console.
    /// </summary>
    public class DefaultExceptionHandler : ICliExceptionHandler
    {
        /// <inheritdoc/>
        public void HandleCliFxException(ICliContext context, CliFxException ex)
        {
            WriteError(context.Console, ex.ToString());
            //ex.ShowHelp = false;
        }

        /// <inheritdoc/>
        public void HandleDirectiveException(ICliContext context, DirectiveException ex)
        {
            WriteError(context.Console, ex.ToString());
        }

        /// <inheritdoc/>
        public void HandleCommandException(ICliContext context, CommandException ex)
        {
            WriteError(context.Console, ex.ToString());
        }

        /// <inheritdoc/>
        public void HandleException(ICliContext context, Exception ex)
        {
            IConsole console = context.Console;

            if (context.CurrentCommand is null)
                WriteError(console, $"Fatal error occured in {context.Metadata.ExecutableName}.");
            else
                WriteError(console, $"Fatal error occured in {context.Metadata.ExecutableName} during execution of '{context.CurrentCommand.Name ?? "default"}' command.");

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
