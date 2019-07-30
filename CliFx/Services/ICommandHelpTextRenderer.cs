using System.Collections.Generic;
using CliFx.Models;

namespace CliFx.Services
{
    public interface ICommandHelpTextRenderer
    {
        void RenderHelpText(ApplicationMetadata applicationMetadata,
            IReadOnlyList<CommandSchema> availableCommandSchemas, CommandSchema matchingCommandSchema = null);
    }
}