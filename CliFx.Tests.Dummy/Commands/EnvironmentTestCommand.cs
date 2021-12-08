using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace CliFx.Tests.Dummy.Commands;

[Command("env-test")]
public class EnvironmentTestCommand : ICommand
{
    [CommandOption("target", EnvironmentVariable = "ENV_TARGET")]
    public string GreetingTarget { get; set; } = "World";

    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine($"Hello {GreetingTarget}!");

        return default;
    }
}