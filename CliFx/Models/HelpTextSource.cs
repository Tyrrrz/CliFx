using System.Collections.Generic;
using CliFx.Internal;

namespace CliFx.Models
{
    /// <summary>
    /// Source information used to generate help text.
    /// </summary>
    public class HelpTextSource
    {
        /// <summary>
        /// Application metadata.
        /// </summary>
        public ApplicationMetadata ApplicationMetadata { get; }

        /// <summary>
        /// Schemas of commands available in the application.
        /// </summary>
        public IReadOnlyList<CommandSchema> AvailableCommandSchemas { get; }

        /// <summary>
        /// Schema of the command for which help text is to be generated.
        /// </summary>
        public CommandSchema TargetCommandSchema { get; }

        /// <summary>
        /// Initializes an instance of <see cref="HelpTextSource"/>.
        /// </summary>
        public HelpTextSource(ApplicationMetadata applicationMetadata,
            IReadOnlyList<CommandSchema> availableCommandSchemas,
            CommandSchema targetCommandSchema)
        {
            ApplicationMetadata = applicationMetadata.GuardNotNull(nameof(applicationMetadata));
            AvailableCommandSchemas = availableCommandSchemas.GuardNotNull(nameof(availableCommandSchemas));
            TargetCommandSchema = targetCommandSchema.GuardNotNull(nameof(targetCommandSchema));
        }
    }
}