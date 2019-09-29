using System.Collections.Generic;
using System.IO;
using CliFx.Services;

namespace CliFx.Tests.Stubs
{
    public class EnvironmentVariablesProviderStub : IEnvironmentVariablesProvider
    {
        public static readonly Dictionary<string, string> EnvironmentVariables = new Dictionary<string, string>
        {
            ["ENV_SINGLE_VALUE"] = "A",
            ["ENV_MULTIPLE_VALUES"] = $"A{Path.PathSeparator}B{Path.PathSeparator}C{Path.PathSeparator}",
            ["ENV_ESCAPED_MULTIPLE_VALUES"] = $"\"A{Path.PathSeparator}B{Path.PathSeparator}C{Path.PathSeparator}\""
        };

        public IReadOnlyDictionary<string, string> GetEnvironmentVariables() => EnvironmentVariables;
    }
}
