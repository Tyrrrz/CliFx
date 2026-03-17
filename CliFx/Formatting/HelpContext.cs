using System.Collections.Generic;
using CliFx.Binding;

namespace CliFx.Formatting;

internal class HelpContext(
    ApplicationMetadata applicationMetadata,
    ApplicationDescriptor applicationDescriptor,
    CommandDescriptor commandDescriptor,
    IReadOnlyDictionary<CommandInputDescriptor, object?> commandDefaultValues
)
{
    public ApplicationMetadata ApplicationMetadata { get; } = applicationMetadata;
    public ApplicationDescriptor ApplicationDescriptor { get; } = applicationDescriptor;
    public CommandDescriptor CommandDescriptor { get; } = commandDescriptor;

    public IReadOnlyDictionary<CommandInputDescriptor, object?> CommandDefaultValues { get; } =
        commandDefaultValues;
}
