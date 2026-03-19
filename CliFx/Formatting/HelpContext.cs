using System.Collections.Generic;
using CliFx.Binding;

namespace CliFx.Formatting;

internal record HelpContext(
    ApplicationMetadata ApplicationMetadata,
    ApplicationDescriptor ApplicationDescriptor,
    CommandDescriptor CommandDescriptor,
    IReadOnlyDictionary<CommandInputDescriptor, object?> CommandDefaultValues
);
