using System.Collections.Generic;

namespace CliFx.Models
{
    public class HelpTextSource
    {
        public ApplicationMetadata ApplicationMetadata { get; }

        public IReadOnlyList<CommandSchema> AvailableCommandSchemas { get; }

        public CommandSchema TargetCommandSchema { get; }

        public HelpTextSource(ApplicationMetadata applicationMetadata,
            IReadOnlyList<CommandSchema> availableCommandSchemas,
            CommandSchema targetCommandSchema)
        {
            ApplicationMetadata = applicationMetadata;
            AvailableCommandSchemas = availableCommandSchemas;
            TargetCommandSchema = targetCommandSchema;
        }
    }
}