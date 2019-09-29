using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Parses environment variable values
    /// </summary>
    public interface IEnvironmentVariablesParser
    {
        /// <summary>
        /// Parse an environment variable value and converts it to a <see cref="CommandOptionInput"/> 
        /// </summary>
        CommandOptionInput GetCommandOptionInputFromEnvironmentVariable(string environmentVariableValue, CommandOptionSchema targetOptionSchema);
    }
}
