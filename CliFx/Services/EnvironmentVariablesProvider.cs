using CliFx.Internal;
using System;
using System.Collections.Generic;
using System.Security;

namespace CliFx.Services
{
    /// <inheritdoc />
    public class EnvironmentVariablesProvider : IEnvironmentVariablesProvider
    {
        /// <summary>
        /// Default delimiter for multiple values
        /// </summary>
        public const char DEFAULT_VALUES_DELIMITER = ',';

        private readonly char _valuesDelimiter;

        /// <summary>
        /// Initializes a new instance of <see cref="EnvironmentVariablesProvider"/> with the values delimiter specified
        /// </summary>
        public EnvironmentVariablesProvider(char valuesDelimiter)
        {
            _valuesDelimiter = valuesDelimiter;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="EnvironmentVariablesProvider"/> with the default values delimiter
        /// </summary>
        public EnvironmentVariablesProvider()
            : this(DEFAULT_VALUES_DELIMITER)
        {
        }

        /// <inheritdoc />
        public IEnumerable<string> GetValues(string variableName)
        {
            variableName.GuardNotNull(nameof(variableName));

            try
            {
                string variableValue = Environment.GetEnvironmentVariable(variableName);

                if (string.IsNullOrWhiteSpace(variableValue)) return null;

                return variableValue.Split(_valuesDelimiter);
            }
            catch (SecurityException)
            {
                return null;
            }
        }
    }
}
