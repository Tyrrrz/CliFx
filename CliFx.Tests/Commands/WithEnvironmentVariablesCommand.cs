using System.Collections.Generic;
using CliFx.Attributes;
using CliFx.Tests.Utils;

namespace CliFx.Tests.Commands
{
    [Command("cmd")]
    public class WithEnvironmentVariablesCommand : SelfSerializingCommandBase
    {
        [CommandOption("opt-a", 'a', EnvironmentVariableName = "ENV_OPT_A")]
        public string? OptA { get; set; }

        [CommandOption("opt-b", 'b', EnvironmentVariableName = "ENV_OPT_B")]
        public IReadOnlyList<string>? OptB { get; set; }
    }
}