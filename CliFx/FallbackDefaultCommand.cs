using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace CliFx;

// Default command for when an application doesn't have one registered.
// It's only used as a stub to show help text when the application is executed
// without specifying a command, or when the specified command fails to resolve.
[Command]
internal partial class FallbackDefaultCommand : ICommand
{
    // Never actually executed. CliFx intercepts this specific command implementation
    // and always shows help text, even when it wasn't explicitly requested.
    [ExcludeFromCodeCoverage]
    public ValueTask ExecuteAsync(IConsole console) => default;
}
