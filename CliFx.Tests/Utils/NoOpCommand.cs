using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace CliFx.Tests.Utils
{
    [Command]
    public class NoOpCommand : ICommand
    {
        public ValueTask ExecuteAsync(IConsole console) => default;
    }
}