using System.Collections.Generic;
using CliFx.Schema;

namespace CliFx.Formatting;

internal class HelpContext(
    ApplicationMetadata applicationMetadata,
    ApplicationSchema applicationSchema,
    CommandSchema commandSchema,
    IReadOnlyDictionary<InputSchema, object?> commandDefaultValues
)
{
    public ApplicationMetadata ApplicationMetadata { get; } = applicationMetadata;

    public ApplicationSchema ApplicationSchema { get; } = applicationSchema;

    public CommandSchema CommandSchema { get; } = commandSchema;

    public IReadOnlyDictionary<InputSchema, object?> CommandDefaultValues { get; } =
        commandDefaultValues;
}
