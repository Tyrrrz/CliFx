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
        new CommandSchema(
            typeof(FallbackDefaultCommand),
            null,
            null,
            [],
            [CommandOptionSchema.ImplicitHelpOption, CommandOptionSchema.ImplicitVersionOption]
        );

    // Never actually executed
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public ValueTask ExecuteAsync(IConsole console) => default;
}
