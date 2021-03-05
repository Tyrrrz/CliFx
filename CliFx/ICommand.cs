﻿using System.Threading.Tasks;

namespace CliFx
{
    /// <summary>
    /// Command, through which the user interacts with the command line application.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Executes the command using the specified implementation of <see cref="IConsole"/>.
        /// </summary>
        /// <remarks>
        /// If the execution of the command is not asynchronous, simply end the method with <code>return default;</code>
        /// </remarks>
        ValueTask ExecuteAsync(IConsole console);
    }
}