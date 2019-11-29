using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliFx.Models
{
    /// <summary>
    /// Defines the target command and the input required for initializing the command.
    /// </summary>
    public class TargetCommandSchema
    {
        /// <summary>
        /// The command schema of the target command.
        /// </summary>
        public CommandSchema? Schema { get; }

        /// <summary>
        /// The positional arguments input for the command.
        /// </summary>
        public IReadOnlyList<string> PositionalArgumentsInput { get; }

        /// <summary>
        /// The command input for the command.
        /// </summary>
        public CommandInput CommandInput { get; }

        /// <summary>
        /// Initializes and instance of <see cref="TargetCommandSchema"/>
        /// </summary>
        public TargetCommandSchema(CommandSchema? schema, IReadOnlyList<string> positionalArgumentsInput, CommandInput commandInput)
        {
            Schema = schema;
            PositionalArgumentsInput = positionalArgumentsInput;
            CommandInput = commandInput;
        }
    }
}
