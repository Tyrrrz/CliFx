using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace CliFx;

// Default command for when an application doesn't have one registered.
// It's only used as a stub to show help text when the application doesn't have its own default command.
[Command]
internal partial class FallbackDefaultCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) =>
        throw new CommandException("Use one of the available named commands.", 1, true);
}
