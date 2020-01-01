using System;
using System.Collections.Generic;

namespace CliFx.Models
{
    /// <summary>
    /// Schema of a defined command.
    /// </summary>
    public interface ICommandSchema
    {
        /// <summary>
        /// Underlying type.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Command name.
        /// </summary>
        string? Name { get; }

        /// <summary>
        /// Command description.
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Command options.
        /// </summary>
        IReadOnlyList<CommandOptionSchema> Options { get; }

        /// <summary>
        /// Command arguments.
        /// </summary>
        IReadOnlyList<CommandArgumentSchema> Arguments { get; }
    }
}