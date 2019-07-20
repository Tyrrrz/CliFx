using System.Collections.Generic;
using CliFx.Models;

namespace CliFx.Services
{
    public interface ICommandSchemaResolver
    {
        IReadOnlyList<CommandSchema> ResolveAllSchemas();
    }
}