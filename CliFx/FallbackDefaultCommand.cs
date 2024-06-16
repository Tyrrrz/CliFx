using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using CliFx.Input;
using CliFx.Schema;

namespace CliFx;

// Fallback command used when the application doesn't have one configured.
// This command is only used as a stub for help text.
[Command]
internal partial class FallbackDefaultCommand : IBindableCommand, ICommandWithHelpOption, ICommandWithVersionOption
{
    [CommandHelpOption]
    public bool IsHelpRequested { get; init; }

    [CommandVersionOption]
    public bool IsVersionRequested { get; init; }

    public void Bind(CommandInput input)
    {
        throw new System.NotImplementedException();
    }

    // Never actually executed
    [ExcludeFromCodeCoverage]
    public ValueTask ExecuteAsync(IConsole console) => default;
}

internal partial class FallbackDefaultCommand
{
    public static CommandSchema Schema { get; } =
        new(typeof(FallbackDefaultCommand), null, null, [], []);
}
