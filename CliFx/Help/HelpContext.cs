using System.Collections.Generic;
using CliFx.Binding;

namespace CliFx.Help;

internal record HelpContext(
    CommandLineApplicationMetadata Metadata,
    CommandRootDescriptor Root,
    CommandDescriptor Command,
    IReadOnlyDictionary<CommandInputDescriptor, object?> CommandDefaultValues
);
