using System.Collections.Generic;
using CliFx.Models;

namespace CliFx.Services
{
    public interface ICommandHelpTextBuilder
    {
        string Build(ApplicationMetadata applicationMetadata,
            IReadOnlyList<CommandSchema> availableCommandSchemas,
            CommandSchema matchingCommandSchema);
    }
}