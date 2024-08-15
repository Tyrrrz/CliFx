using System.Collections.Generic;
using CliFx.Schema;

namespace CliFx.Formatting;

internal class HelpContext(
    ApplicationMetadata metadata,
    ApplicationSchema application,
    CommandSchema command,
    IReadOnlyDictionary<CommandInputSchema, object?> commandDefaultValues
)
{
    public ApplicationMetadata Metadata { get; } = metadata;

    public ApplicationSchema Application { get; } = application;

    public CommandSchema Command { get; } = command;

    public IReadOnlyDictionary<CommandInputSchema, object?> CommandDefaultValues { get; } =
        commandDefaultValues;
}
