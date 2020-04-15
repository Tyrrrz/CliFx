using System;

namespace CliFx.Exceptions
{
    /// <summary>
    /// Enumerates the possible ways of presenting error messages.
    /// This is a Flags enum, so if we want to show exception message
    /// and the helptext we can OR them together like so: `ExceptionMessage | HelpText`
    /// </summary>
    [Flags]
    public enum CommandErrorDisplayOptions
    {
        
        /// <summary>
        /// Show the exception message.
        /// </summary>
        ExceptionMessage = 1,

        /// <summary>
        /// Show only the help text associated with this command.
        /// As if the user entered the -h/--help option.
        /// </summary>
        HelpText = 2,

        /// <summary>
        /// Do not display anything.
        /// </summary>
        None = 4,
    }
}