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

        /// <summary>
        /// Get the target command schema. The target command is the most specific command that matches the unbound input arguments.
        /// </summary>
        CommandCandidate GetTargetCommandSchema(IReadOnlyList<CommandSchema> availableCommandSchemas, CommandInput commandInput);
    }
}