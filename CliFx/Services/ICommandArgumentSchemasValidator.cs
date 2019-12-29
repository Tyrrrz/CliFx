using System.Collections.Generic;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Validates command arguments.
    /// </summary>
    public interface ICommandArgumentSchemasValidator
    {
        /// <summary>
        /// Validate the given command arguments.
        /// </summary>
        IEnumerable<ValidationError> ValidateArgumentSchemas(IReadOnlyCollection<CommandArgumentSchema> commandArgumentSchemas);
    }
}