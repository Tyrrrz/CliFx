using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace CliFx.Tests.Utils;

[Command]
internal class NoOpCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}
