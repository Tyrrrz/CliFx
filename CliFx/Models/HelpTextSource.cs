using System.Collections.Generic;

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
        public IReadOnlyList<ICommandSchema> AvailableCommandSchemas { get; }

        /// <summary>
        /// Schema of the command for which help text is to be generated.
        /// </summary>
        public ICommandSchema? TargetCommandSchema { get; }

        /// <summary>
        /// Initializes an instance of <see cref="HelpTextSource"/>.
        /// </summary>
        public HelpTextSource(ApplicationMetadata applicationMetadata,
            IReadOnlyList<ICommandSchema> availableCommandSchemas,
            ICommandSchema? targetCommandSchema)
        {
            ApplicationMetadata = applicationMetadata;
            AvailableCommandSchemas = availableCommandSchemas;
            TargetCommandSchema = targetCommandSchema;
        }
    }
}