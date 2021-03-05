using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace CliFx.Tests.Commands
{
    [Command("named sub", Description = "Named sub command description")]
    public class NamedSubCommand : ICommand
    {
        public const string ExpectedOutputText = nameof(NamedSubCommand);

        public ValueTask ExecuteAsync(IConsole console)
        {
            console.Output.WriteLine(ExpectedOutputText);
            return default;
        }
    }
}