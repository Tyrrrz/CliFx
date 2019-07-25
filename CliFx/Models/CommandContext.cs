using System.Collections.Generic;
using CliFx.Services;

namespace CliFx.Models
{
    public class CommandContext
    {
        public ApplicationMetadata ApplicationMetadata { get; }

        public IReadOnlyList<CommandSchema> AvailableCommandSchemas { get; }

        public CommandSchema MatchingCommandSchema { get; }

        public CommandInput CommandInput { get; }

        public IConsoleWriter Output { get; }

        public IConsoleWriter Error { get; }

        public CommandContext(ApplicationMetadata applicationMetadata,
            IReadOnlyList<CommandSchema> availableCommandSchemas, CommandSchema matchingCommandSchema,
            CommandInput commandInput, IConsoleWriter output, IConsoleWriter error)
        {
            ApplicationMetadata = applicationMetadata;
            AvailableCommandSchemas = availableCommandSchemas;
            MatchingCommandSchema = matchingCommandSchema;
            CommandInput = commandInput;
            Output = output;
            Error = error;
        }
    }
}