using System.Collections.Generic;
using CliFx.Binding;

namespace CliFx.Formatting;

internal record HelpContext(
    CommandLineApplicationMetadata ApplicationMetadata,
    CommandRootDescriptor ApplicationDescriptor,
    CommandDescriptor CommandDescriptor,
    IReadOnlyDictionary<CommandInputDescriptor, object?> CommandDefaultValues
);
