using CliFx.Attributes;

namespace CliFx.Tests.Commands.Invalid
{
    [Command("cmd")]
    public class DuplicateOptionEnvironmentVariableNamesCommand : SelfSerializeCommandBase
    {
        [CommandOption("option-a", EnvironmentVariableName = "ENV_VAR")]
        public string? OptionA { get; set; }

        [CommandOption("option-b", EnvironmentVariableName = "ENV_VAR")]
        public string? OptionB { get; set; }
    }
}