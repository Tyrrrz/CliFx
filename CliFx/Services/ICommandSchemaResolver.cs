using System;
using System.Collections.Generic;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Resolves command schemas.
    /// </summary>
    public interface ICommandSchemaResolver
    {
        /// <summary>
        /// Resolves schemas of specified command types.
        /// </summary>
        IReadOnlyList<CommandSchema> GetCommandSchemas(IReadOnlyList<Type> commandTypes);
    }
}