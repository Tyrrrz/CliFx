using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using CliFx.Schema;

namespace CliFx;

// Fallback command used when the application doesn't have one configured.
// This command is only used as a stub for help text.
[Command]
internal class FallbackDefaultCommand : ICommand
{
    public static CommandSchema Schema { get; } =
        CommandSchema.Resolve(typeof(FallbackDefaultCommand));

    // Never actually executed
    [ExcludeFromCodeCoverage]
    public ValueTask ExecuteAsync(IConsole console) => default;
}