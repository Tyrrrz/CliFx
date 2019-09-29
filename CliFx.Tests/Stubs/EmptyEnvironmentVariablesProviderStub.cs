using System.Collections.Generic;
using CliFx.Services;

namespace CliFx.Tests.Stubs
{
    public class EmptyEnvironmentVariablesProviderStub : IEnvironmentVariablesProvider
    {
        public IReadOnlyDictionary<string, string> GetEnvironmentVariables() => new Dictionary<string, string>();
    }
}
