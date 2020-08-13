namespace CliFx.Tests.Dummy.Commands
{
    using System.Threading.Tasks;
    using CliFx.Attributes;

    [Command]
    public class HelloWorldCommand : ICommand
    {
        [CommandOption("target", EnvironmentVariableName = "ENV_TARGET")]
        public string Target { get; set; } = "World";

        public ValueTask ExecuteAsync(IConsole console)
        {
            console.Output.WriteLine($"Hello {Target}!");

            return default;
        }
    }
}