using System.Collections.Generic;
using CliFx.Schema;

namespace CliFx.Formatting;

internal class HelpContext(
    ApplicationMetadata applicationMetadata,
    ApplicationSchema applicationSchema,
    CommandSchema commandSchema,
    IReadOnlyDictionary<IMemberSchema, object?> commandDefaultValues
)
{
    public ApplicationMetadata ApplicationMetadata { get; } = applicationMetadata;

    public ApplicationSchema ApplicationSchema { get; } = applicationSchema;

    public CommandSchema CommandSchema { get; } = commandSchema;

    public IReadOnlyDictionary<IMemberSchema, object?> CommandDefaultValues { get; } =
        commandDefaultValues;
}
