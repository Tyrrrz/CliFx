using System;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Extensibility;
using CliFx.Infrastructure;
using CliFx.Schema;

namespace CliFx;

// Fallback command used when the application doesn't have one configured.
// This command is only used as a stub for help text.
[Command]
internal class FallbackDefaultCommand : ICommandWithHelpOption, ICommandWithVersionOption
{
    public bool IsHelpRequested { get; set; }
    public bool IsVersionRequested { get; set; }

    public static CommandSchema Schema { get; } =
        new CommandSchema(
            typeof(FallbackDefaultCommand),
            null,
            null,
            [],
            [
                new CommandOptionSchema<FallbackDefaultCommand, bool>(
                    new PropertyBinding<FallbackDefaultCommand, bool>(
                        c => c.IsHelpRequested,
                        (c, v) => c.IsHelpRequested = v
                    ),
                    false,
                    "help",
                    'h',
                    null,
                    false,
                    "Shows help text.",
                    new BoolBindingConverter(),
                    Array.Empty<IBindingValidator>()
                ),
                new CommandOptionSchema<FallbackDefaultCommand, bool>(
                    new PropertyBinding<FallbackDefaultCommand, bool>(
                        c => c.IsVersionRequested,
                        (c, v) => c.IsVersionRequested = v
                    ),
                    false,
                    "version",
                    null,
                    null,
                    false,
                    "Shows version information.",
                    new BoolBindingConverter(),
                    Array.Empty<IBindingValidator>()
                ),
            ]
        );

    // Never actually executed
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public ValueTask ExecuteAsync(IConsole console) => default;
}
