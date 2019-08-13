using System;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Resolves command schemas.
    /// </summary>
    public interface ICommandSchemaResolver
    {
        /// <summary>
        /// Resolves schema of a command of specified type.
        /// </summary>
        CommandSchema GetCommandSchema(Type commandType);
    }
}