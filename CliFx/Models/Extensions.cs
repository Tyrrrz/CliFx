using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Internal;

namespace CliFx.Models
{
    public static class Extensions
    {
        public static bool IsCommandSpecified(this CommandInput commandInput) => !commandInput.CommandName.IsNullOrWhiteSpace();

        public static bool IsEmpty(this CommandInput commandInput) => !commandInput.IsCommandSpecified() && !commandInput.Options.Any();

        public static bool IsHelpOption(this CommandOptionInput optionInput) =>
            string.Equals(optionInput.Alias, "help", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(optionInput.Alias, "h", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(optionInput.Alias, "?", StringComparison.OrdinalIgnoreCase);

        public static bool IsVersionOption(this CommandOptionInput optionInput) =>
            string.Equals(optionInput.Alias, "version", StringComparison.OrdinalIgnoreCase);

        public static bool IsHelpRequested(this CommandInput commandInput) => commandInput.Options.Any(o => o.IsHelpOption());

        public static bool IsVersionRequested(this CommandInput commandInput) => commandInput.Options.Any(o => o.IsVersionOption());

        public static bool IsDefault(this CommandSchema commandSchema) => commandSchema.Name.IsNullOrWhiteSpace();

        public static CommandSchema FindByNameOrNull(this IEnumerable<CommandSchema> commandSchemas, string name) =>
            commandSchemas.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

        public static IReadOnlyList<CommandSchema> FindSubCommandSchemas(this IEnumerable<CommandSchema> commandSchemas,
            string parentName)
        {
            // For a command with no name, every other command is its subcommand
            if (parentName.IsNullOrWhiteSpace())
                return commandSchemas.Where(c => !c.Name.IsNullOrWhiteSpace()).ToArray();

            // For a named command, commands that are prefixed by its name are its subcommands
            return commandSchemas.Where(c => !c.Name.IsNullOrWhiteSpace())
                .Where(c => c.Name.StartsWith(parentName + " ", StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }
        public static CommandOptionSchema FindByAliasOrNull(this IEnumerable<CommandOptionSchema> optionSchemas, string alias) =>
            optionSchemas.FirstOrDefault(o => o.GetAliases().Contains(alias, StringComparer.OrdinalIgnoreCase));

        public static IReadOnlyList<string> GetAliases(this CommandOptionSchema optionSchema)
        {
            var result = new List<string>();

            if (!optionSchema.Name.IsNullOrWhiteSpace())
                result.Add(optionSchema.Name);

            if (optionSchema.ShortName != null)
                result.Add(optionSchema.ShortName.Value.AsString());

            return result;
        }

        public static IReadOnlyList<string> GetAliasesWithPrefixes(this CommandOptionSchema optionSchema)
        {
            var result = new List<string>();

            if (!optionSchema.Name.IsNullOrWhiteSpace())
                result.Add("--" + optionSchema.Name);

            if (optionSchema.ShortName != null)
                result.Add("-" + optionSchema.ShortName.Value.AsString());

            return result;
        }
    }
}