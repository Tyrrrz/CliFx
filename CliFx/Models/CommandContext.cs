using System.Collections.Generic;
using CliFx.Services;

namespace CliFx.Models
{
    public class CommandContext
    {
        public CommandInput CommandInput { get; }

        public IReadOnlyList<CommandSchema> AvailableCommandSchemas { get; }

        public CommandSchema MatchingCommandSchema { get; }

        public IConsoleWriter Output { get; }

        public IConsoleWriter Error { get; }

        public CommandContext(CommandInput commandInput,
            IReadOnlyList<CommandSchema> availableCommandSchemas, CommandSchema matchingCommandSchema,
            IConsoleWriter output, IConsoleWriter error)
        {
            CommandInput = commandInput;
            AvailableCommandSchemas = availableCommandSchemas;
            MatchingCommandSchema = matchingCommandSchema;
            Output = output;
            Error = error;
        }
    }
}