using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Internal;

namespace CliFx.Models
{
    /// <summary>
    /// Extensions for <see cref="Models"/>.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets whether a command was specified in the input.
        /// </summary>
        public static bool IsCommandSpecified(this CommandInput commandInput)
        {
            commandInput.GuardNotNull(nameof(commandInput));
            return !commandInput.CommandName.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Gets whether help was requested in the input.
        /// </summary>
        public static bool IsHelpRequested(this CommandInput commandInput)
        {
            commandInput.GuardNotNull(nameof(commandInput));

            var firstOptionAlias = commandInput.Options.FirstOrDefault()?.Alias;

            return string.Equals(firstOptionAlias, "help", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(firstOptionAlias, "h", StringComparison.Ordinal) ||
                   string.Equals(firstOptionAlias, "?", StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets whether version information was requested in the input.
        /// </summary>
        public static bool IsVersionRequested(this CommandInput commandInput)
        {
            commandInput.GuardNotNull(nameof(commandInput));

            var firstOptionAlias = commandInput.Options.FirstOrDefault()?.Alias;

            return string.Equals(firstOptionAlias, "version", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets whether this command is the default command, i.e. without a name.
        /// </summary>
        public static bool IsDefault(this CommandSchema commandSchema)
        {
            commandSchema.GuardNotNull(nameof(commandSchema));
            return commandSchema.Name.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Finds a command that has specified name, or null if not found.
        /// </summary>
        public static CommandSchema FindByName(this IReadOnlyList<CommandSchema> commandSchemas, string commandName)
        {
            commandSchemas.GuardNotNull(nameof(commandSchemas));
            return commandSchemas.FirstOrDefault(c => string.Equals(c.Name, commandName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Finds parent command to the command that has specified name, or null if not found.
        /// </summary>
        public static CommandSchema FindParent(this IReadOnlyList<CommandSchema> commandSchemas, string commandName)
        {
            commandSchemas.GuardNotNull(nameof(commandSchemas));

            // If command has no name, it's the default command so it doesn't have a parent
            if (commandName.IsNullOrWhiteSpace())
                return null;

            // Repeatedly cut off individual words from the name until we find a command with that name
            var temp = commandName;
            while (temp.Contains(" "))
            {
                temp = temp.SubstringUntilLast(" ");

                var parent = commandSchemas.FindByName(temp);
                if (parent != null)
                    return parent;
            }

            // If no parent is matched by name, then the parent is the default command
            return commandSchemas.FirstOrDefault(c => c.IsDefault());
        }

        /// <summary>
        /// Finds an option that matches specified alias, or null if not found.
        /// </summary>
        public static CommandOptionSchema FindByAlias(this IReadOnlyList<CommandOptionSchema> optionSchemas, string alias)
        {
            optionSchemas.GuardNotNull(nameof(optionSchemas));
            alias.GuardNotNull(nameof(alias));

            foreach (var optionSchema in optionSchemas)
            {
                // Compare against name. Case is ignored.
                var matchesByName =
                    !optionSchema.Name.IsNullOrWhiteSpace() &&
                    string.Equals(optionSchema.Name, alias, StringComparison.OrdinalIgnoreCase);

                // Compare against short name. Case is NOT ignored.
                var matchesByShortName =
                    optionSchema.ShortName != null &&
                    alias.Length == 1 &&
                    alias[0] == optionSchema.ShortName;

                if (matchesByName || matchesByShortName)
                    return optionSchema;
            }

            return null;
        }
    }
}