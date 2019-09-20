using CliFx.Services;
using System;
using System.Collections.Generic;

namespace CliFx.Tests.Stubs
{
    public class EnvironmentVariablesProviderStub : IEnvironmentVariablesProvider
    {
        private readonly Dictionary<string, IReadOnlyList<string>> _variablesWithValues = new Dictionary<string, IReadOnlyList<string>>
        {
            ["ENV_VAR_1"] = new[] { "A" },
            ["ENV_VAR_2"] = new[] { "A", "B", "C" }
        };

        public IReadOnlyList<string> GetValues(string variableName)
        {
            if (variableName == null) return null;

            if (!_variablesWithValues.ContainsKey(variableName))
                throw new NotSupportedException($"${variableName} does not exist, did you forget to add it to the variables dictionary?");

            return _variablesWithValues[variableName];
        }
    }
}
