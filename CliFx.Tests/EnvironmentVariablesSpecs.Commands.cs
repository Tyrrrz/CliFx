using System.Collections.Generic;
using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests
{
    public partial class EnvironmentVariablesSpecs
    {
        [Command]
        private class EnvironmentVariableCollectionCommand : ICommand
        {
            [CommandOption("opt", EnvironmentVariableName = "ENV_OPT")]
            public IReadOnlyList<string>? Option { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }

        [Command]
        private class EnvironmentVariableCommand : ICommand
        {
            [CommandOption("opt", EnvironmentVariableName = "ENV_OPT")]
            public string? Option { get; set; }

            public ValueTask ExecuteAsync(IConsole console) => default;
        }
    }
}