using System.Threading.Tasks;
using CliFx.Infrastructure;

namespace CliFx.Tests.Utils;

[Command]
internal partial class NoOpCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}
