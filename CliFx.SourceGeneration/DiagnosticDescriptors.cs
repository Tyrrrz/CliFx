using Microsoft.CodeAnalysis;

namespace CliFx.SourceGeneration;

internal static class DiagnosticDescriptors
{
    public static DiagnosticDescriptor CommandMustBePartial { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandMustBePartial)}",
            "Command types must be declared as `partial`",
            "This type (and all its containing types, if present) must be declared as `partial` in order to be a valid command.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor CommandMustImplementInterface { get; } =
        new(
            $"{nameof(CliFx)}_{nameof(CommandMustImplementInterface)}",
            $"Commands must implement the `{KnownSymbolNames.CliFxCommandInterface}` interface",
            $"This type must implement the `{KnownSymbolNames.CliFxCommandInterface}` interface in order to be a valid command.",
            "CliFx",
            DiagnosticSeverity.Error,
            true
        );
}
