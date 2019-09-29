using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;

namespace CliFx.Services
{
    /// <inheritdoc />
    public class EnvironmentVariablesProvider : IEnvironmentVariablesProvider
    {
        /// <inheritdoc />
        public IReadOnlyDictionary<string, string> GetEnvironmentVariables()
        {
            try
            {
                var environmentVariables = Environment.GetEnvironmentVariables();

                //Constructing the dictionary manually allows to specify a key comparer that ignores case
                //This allows to ignore casing when looking for a fallback environment variable of an option
                var environmentVariablesAsDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                //Type DictionaryEntry must be explicitly used otherwise it will enumerate as a collection of objects
                foreach (DictionaryEntry environmentVariable in environmentVariables)
                {
                    environmentVariablesAsDictionary.Add(environmentVariable.Key.ToString(), environmentVariable.Value.ToString());
                }

                return environmentVariablesAsDictionary;
            }
            catch (SecurityException)
            {
                return new Dictionary<string, string>();
            }
        }
    }
}
