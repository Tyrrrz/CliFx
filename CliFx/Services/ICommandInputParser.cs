using System.Collections.Generic;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Parses command line arguments.
    /// </summary>
    public interface ICommandInputParser
    {
        /// <summary>
        /// Parses specified command line arguments.
        /// </summary>
        CommandInput ParseInput(IReadOnlyList<string> commandLineArguments);
    }
}