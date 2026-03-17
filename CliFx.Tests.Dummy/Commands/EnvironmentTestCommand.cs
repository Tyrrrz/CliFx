using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace CliFx.Tests.Dummy.Commands;

[Command("env-test")]
public partial class EnvironmentTestCommand : ICommand
{
    [CommandOption("target", EnvironmentVariable = "ENV_TARGET")]
    public string GreetingTarget { get; set; } = "World";

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.WriteLine($"Hello {GreetingTarget}!");
        return default;
    }
}
