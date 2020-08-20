using System.Threading.Tasks;
using CliFx.Attributes;

namespace CliFx.Tests.Commands
{
    [Command(Description = "Default command description")]
    public class DefaultCommand : ICommand
    {
        public const string ExpectedOutputText = nameof(DefaultCommand);

        public ValueTask ExecuteAsync(IConsole console)
        {
            console.Output.WriteLine(ExpectedOutputText);
            return default;
        }
    }
}