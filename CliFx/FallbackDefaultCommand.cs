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
#pragma warning disable IL2026
    public static CommandSchema Schema { get; } =
        CommandSchema.Resolve(typeof(FallbackDefaultCommand));
#pragma warning restore IL2026

    // Never actually executed
    [ExcludeFromCodeCoverage]
    public ValueTask ExecuteAsync(IConsole console) => default;
}
