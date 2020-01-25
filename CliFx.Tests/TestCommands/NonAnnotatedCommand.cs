using System.Threading.Tasks;

namespace CliFx.Tests.TestCommands
{
    public class NonAnnotatedCommand : ICommand
    {
        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}