using System.Threading.Tasks;
using CliFx.Services;

namespace CliFx.Tests.TestCommands
{
    public class NonAnnotatedCommand : ICommand
    {
        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}