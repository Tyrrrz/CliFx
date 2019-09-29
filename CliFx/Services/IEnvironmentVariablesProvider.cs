using System.Collections.Generic;

namespace CliFx.Services
{
    /// <summary>
    /// Provides environment variable values
    /// </summary>
    public interface IEnvironmentVariablesProvider
    {
        /// <summary>
        /// Returns all the environment variables available.
        /// </summary>
        /// <remarks>If the User is not allowed to read environment variables it will return an empty dictionary.</remarks>
        IReadOnlyDictionary<string, string> GetEnvironmentVariables();
    }
}
