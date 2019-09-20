using CliFx.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CliFx.Models
{
    /// <summary>
    /// Extensions for <see cref="Models"/>.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Finds a command that has specified name, or null if not found.
        /// </summary>
        public static CommandSchema FindByName(this IReadOnlyList<CommandSchema> commandSchemas, string commandName)
        {
            commandSchemas.GuardNotNull(nameof(commandSchemas));

            // If looking for default command, don't compare names directly
            // ...because null and empty are both valid names for default command
            if (commandName.IsNullOrWhiteSpace())
                return commandSchemas.FirstOrDefault(c => c.IsDefault());

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
        /// Determines whether an option schema matches specified alias.
        /// </summary>
        public static bool MatchesAlias(this CommandOptionSchema optionSchema, string alias)
        {
            optionSchema.GuardNotNull(nameof(optionSchema));
            alias.GuardNotNull(nameof(alias));

            // Compare against name. Case is ignored.
            var matchesByName =
                !optionSchema.Name.IsNullOrWhiteSpace() &&
                string.Equals(optionSchema.Name, alias, StringComparison.OrdinalIgnoreCase);

            // Compare against short name. Case is NOT ignored.
            var matchesByShortName =
                optionSchema.ShortName != null &&
                alias.Length == 1 && alias[0] == optionSchema.ShortName;

            return matchesByName || matchesByShortName;
        }

        /// <summary>
        /// Finds an option that matches specified alias, or null if not found.
        /// </summary>
        public static CommandOptionSchema FindByAlias(this IReadOnlyList<CommandOptionSchema> optionSchemas, string alias)
        {
            optionSchemas.GuardNotNull(nameof(optionSchemas));
            alias.GuardNotNull(nameof(alias));

            return optionSchemas.FirstOrDefault(o => o.MatchesAlias(alias));
        }

        /// <summary>
        /// Finds an option input that matches the option schema specified, or null if not found.
        /// </summary>
        public static CommandOptionInput FindByOptionSchema(this IReadOnlyList<CommandOptionInput> optionInputs, CommandOptionSchema optionSchema)
        {
            optionInputs.GuardNotNull(nameof(optionInputs));
            optionSchema.GuardNotNull(nameof(optionSchema));

            return optionInputs.FirstOrDefault(o => optionSchema.MatchesAlias(o.Alias));
        }

        /// <summary>
        /// Gets valid aliases for the option.
        /// </summary>
        public static IReadOnlyList<string> GetAliases(this CommandOptionSchema optionSchema)
        {
            var result = new List<string>(2);

            if (!optionSchema.Name.IsNullOrWhiteSpace())
                result.Add(optionSchema.Name);

            if (optionSchema.ShortName != null)
                result.Add(optionSchema.ShortName.Value.AsString());

            return result;
        }

        /// <summary>
        /// Gets whether a command was specified in the input.
        /// </summary>
        public static bool IsCommandSpecified(this CommandInput commandInput)
        {
            commandInput.GuardNotNull(nameof(commandInput));
            return !commandInput.CommandName.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Gets whether debug directive was specified in the input.
        /// </summary>
        public static bool IsDebugDirectiveSpecified(this CommandInput commandInput)
        {
            commandInput.GuardNotNull(nameof(commandInput));
            return commandInput.Directives.Contains("debug", StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets whether preview directive was specified in the input.
        /// </summary>
        public static bool IsPreviewDirectiveSpecified(this CommandInput commandInput)
        {
            commandInput.GuardNotNull(nameof(commandInput));
            return commandInput.Directives.Contains("preview", StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets whether help option was specified in the input.
        /// </summary>
        public static bool IsHelpOptionSpecified(this CommandInput commandInput)
        {
            commandInput.GuardNotNull(nameof(commandInput));

            var firstOption = commandInput.Options.FirstOrDefault();
            return firstOption != null && CommandOptionSchema.HelpOption.MatchesAlias(firstOption.Alias);
        }

        /// <summary>
        /// Gets whether version option was specified in the input.
        /// </summary>
        public static bool IsVersionOptionSpecified(this CommandInput commandInput)
        {
            commandInput.GuardNotNull(nameof(commandInput));

            var firstOption = commandInput.Options.FirstOrDefault();
            return firstOption != null && CommandOptionSchema.VersionOption.MatchesAlias(firstOption.Alias);
        }

        /// <summary>
        /// Gets whether this command is the default command, i.e. without a name.
        /// </summary>
        public static bool IsDefault(this CommandSchema commandSchema)
        {
            commandSchema.GuardNotNull(nameof(commandSchema));
            return commandSchema.Name.IsNullOrWhiteSpace();
        }
    }
}