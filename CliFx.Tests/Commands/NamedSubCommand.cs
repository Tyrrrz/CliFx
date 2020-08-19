using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.Commands
{
    [Command("named sub", Description = nameof(NamedSubCommand))]
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