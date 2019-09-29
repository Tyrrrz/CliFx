﻿using System.IO;
using System.Linq;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    /// <inheritdoct />
    public class EnvironmentVariablesParser : IEnvironmentVariablesParser
    {
        /// <inheritdoct />
        public CommandOptionInput GetCommandOptionInputFromEnvironmentVariable(string environmentVariableValue, CommandOptionSchema targetOptionSchema)
        {
            environmentVariableValue.GuardNotNull(nameof(environmentVariableValue));
            targetOptionSchema.GuardNotNull(nameof(targetOptionSchema));

            //If the option is not a collection do not split environment variable values
            var optionIsCollection = targetOptionSchema.Property.PropertyType.IsCollection();

            if (!optionIsCollection) return new CommandOptionInput(targetOptionSchema.EnvironmentVariableName, environmentVariableValue);

            //If the option is a collection split the values using System separator, empty values are discarded
            var environmentVariableValues = environmentVariableValue.Split(Path.PathSeparator)
                .Where(v => !v.IsNullOrWhiteSpace())
                .ToList();

            return new CommandOptionInput(targetOptionSchema.EnvironmentVariableName, environmentVariableValues);
        }
    }
}
