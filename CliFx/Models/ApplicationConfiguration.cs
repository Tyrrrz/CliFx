using System;
using System.Collections.Generic;
using CliFx.Internal;

namespace CliFx.Models
{
    /// <summary>
    /// Configuration of an application.
    /// </summary>
    public class ApplicationConfiguration
    {
        /// <summary>
        /// Command types defined in the application.
        /// </summary>
        public IReadOnlyList<Type> CommandTypes { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ApplicationConfiguration"/>.
        /// </summary>
        public ApplicationConfiguration(IReadOnlyList<Type> commandTypes)
        {
            CommandTypes = commandTypes.GuardNotNull(nameof(commandTypes));
        }
    }
}