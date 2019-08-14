using System.Collections.Generic;
using System.Threading.Tasks;

namespace CliFx
{
    /// <summary>
    /// Entry point for a command line application.
    /// </summary>
    public interface ICliApplication
    {
        /// <summary>
        /// Runs application with specified command line arguments and returns an exit code.
        /// </summary>
        Task<int> RunAsync(IReadOnlyList<string> commandLineArguments);
    }
}