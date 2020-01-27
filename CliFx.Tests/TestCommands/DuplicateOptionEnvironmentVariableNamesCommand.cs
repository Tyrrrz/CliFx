using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.TestCommands
{
    [Command]
    public class DuplicateOptionEnvironmentVariableNamesCommand : ICommand
    {
        [CommandOption("option-a", EnvironmentVariableName = "ENV_VAR")]
        public string? OptionA { get; set; }

        [CommandOption("option-b", EnvironmentVariableName = "ENV_VAR")]
        public string? OptionB { get; set; }

        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}