using System.IO;
using System.Linq;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    /// <inheritdoct />
    public class EnvironmentVariablesParser : IEnvironmentVariablesParser
    {
        /// <inheritdoct />
        public CommandOptionInput GetCommandOptionInputFromEnvironmentVariable(string environmentVariableName, string environmentVariableValue, CommandOptionSchema targetOptionSchema)
        {
            //If the option is not a collection do not split environment variable values
            var optionIsCollection = targetOptionSchema.Property != null && targetOptionSchema.Property.PropertyType.IsCollection();

            if (!optionIsCollection) return new CommandOptionInput(environmentVariableName, environmentVariableValue);

            //If the option is a collection split the values using System separator, empty values are discarded
            var environmentVariableValues = environmentVariableValue.Split(Path.PathSeparator)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();

            return new CommandOptionInput(environmentVariableName, environmentVariableValues);
        }
    }
}
