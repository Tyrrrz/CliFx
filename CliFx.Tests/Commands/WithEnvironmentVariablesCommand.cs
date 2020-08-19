using System.Collections.Generic;
using CliFx.Attributes;

namespace CliFx.Tests.Commands
{
    [Command("cmd")]
    public class WithEnvironmentVariablesCommand : SelfSerializeCommandBase
    {
        [CommandOption("opt-a", 'a', EnvironmentVariableName = "ENV_OPT_A")]
        public string? OptA { get; set; }

        [CommandOption("opt-b", 'b', EnvironmentVariableName = "ENV_OPT_B")]
        public IReadOnlyList<string>? OptB { get; set; }
    }
}