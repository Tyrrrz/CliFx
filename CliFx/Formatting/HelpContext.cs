using System.Collections.Generic;
using CliFx.Schema;

namespace CliFx.Formatting;

internal class HelpContext
{
    public ApplicationMetadata ApplicationMetadata { get; }

    public ApplicationSchema ApplicationSchema { get; }

    public CommandSchema CommandSchema { get; }

    public IReadOnlyDictionary<IMemberSchema, object?> CommandDefaultValues { get; }

    public HelpContext(
        ApplicationMetadata applicationMetadata,
        ApplicationSchema applicationSchema,
        CommandSchema commandSchema,
        IReadOnlyDictionary<IMemberSchema, object?> commandDefaultValues
    )
    {
        ApplicationMetadata = applicationMetadata;
        ApplicationSchema = applicationSchema;
        CommandSchema = commandSchema;
        CommandDefaultValues = commandDefaultValues;
    }
}
