using System.Collections.Generic;

namespace CliFx.Models
{
    public class CommandContext
    {
        public IReadOnlyList<CommandSchema> AvailableCommandSchemas { get; }

        public CommandSchema CommandSchema { get; }

        public CommandContext(IReadOnlyList<CommandSchema> availableCommandSchemas, CommandSchema commandSchema)
        {
            AvailableCommandSchemas = availableCommandSchemas;
            CommandSchema = commandSchema;
        }
    }
}