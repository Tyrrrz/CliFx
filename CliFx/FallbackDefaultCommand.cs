using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace CliFx;

// Fallback command used when the application doesn't have one configured.
// This command is only used as a stub for help text.
// The Schema property, IsHelpRequested, and IsVersionRequested are source-generated.
[Command]
internal partial class FallbackDefaultCommand : ICommand
{
    // Never actually executed
    [ExcludeFromCodeCoverage]
    public ValueTask ExecuteAsync(IConsole console) => default;
}
