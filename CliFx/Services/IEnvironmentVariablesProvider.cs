using System.Collections.Generic;

namespace CliFx.Services
{
    /// <summary>
    /// Provides environment variable values
    /// </summary>
    public interface IEnvironmentVariablesProvider
    {
        /// <summary>
        /// Returns one or more values for the environment variable provided, otherwise returns null.
        /// </summary>
        /// <remarks>If the User is not allowed to read environment variables it will return null.</remarks>
        IReadOnlyList<string> GetValues(string variableName);
    }
}
