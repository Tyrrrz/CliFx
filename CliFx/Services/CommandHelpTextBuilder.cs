using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    // TODO: add color
    public class CommandHelpTextBuilder : ICommandHelpTextBuilder
    {
        private IReadOnlyList<string> GetOptionAliasesWithPrefixes(CommandOptionSchema optionSchema)
        {
            var result = new List<string>();

            if (!optionSchema.Name.IsNullOrWhiteSpace())
                result.Add("--" + optionSchema.Name);

            if (optionSchema.ShortName != null)
                result.Add("-" + optionSchema.ShortName.Value);

            return result;
        }

        private IReadOnlyList<CommandSchema> GetChildCommandSchemas(IReadOnlyList<CommandSchema> availableCommandSchemas,
            CommandSchema parentCommandSchema)
        {
            // TODO: this doesn't really work properly, it shows all descendants instead of direct children
            var prefix = !parentCommandSchema.Name.IsNullOrWhiteSpace() ? parentCommandSchema.Name + " " : "";

            return availableCommandSchemas
                .Where(c => !c.Name.IsNullOrWhiteSpace() && c.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        private void AddDescription(StringBuilder buffer, CommandSchema commands)
        {
            if (commands.Description.IsNullOrWhiteSpace())
                return;

            buffer.AppendLine("Description:");

            buffer.Append("  ");
            buffer.AppendLine(commands.Description);

            buffer.AppendLine();
        }

        private void AddUsage(StringBuilder buffer, ApplicationMetadata applicationMetadata, CommandSchema command,
            IReadOnlyList<CommandSchema> subCommands)
        {
            buffer.AppendLine("Usage:");

            buffer.Append("  ");
            buffer.Append(applicationMetadata.ExecutableName);

            if (!command.Name.IsNullOrWhiteSpace())
            {
                buffer.Append(' ');
                buffer.Append(command.Name);
            }

            if (subCommands.Any())
            {
                buffer.Append(' ');
                buffer.Append("[command]");
            }

            buffer.Append(' ');
            buffer.Append("[options]");

            buffer.AppendLine().AppendLine();
        }

        private void AddOptions(StringBuilder buffer, CommandSchema command)
        {
            buffer.AppendLine("Options:");

            foreach (var option in command.Options)
            {
                buffer.Append(option.IsRequired ? "* " : "  ");

                buffer.Append(GetOptionAliasesWithPrefixes(option).JoinToString("|"));

                if (!option.Description.IsNullOrWhiteSpace())
                {
                    buffer.Append("  ");
                    buffer.Append(option.Description);
                }

                buffer.AppendLine();
            }

            // Help option
            {
                buffer.Append("  ");
                buffer.Append("--help|-h");
                buffer.Append("  ");
                buffer.Append("Shows helps text.");
                buffer.AppendLine();
            }

            // Version option
            if (command.Name.IsNullOrWhiteSpace())
            {
                buffer.Append("  ");
                buffer.Append("--version");
                buffer.Append("  ");
                buffer.Append("Shows application version.");
                buffer.AppendLine();
            }

            buffer.AppendLine();
        }

        private void AddSubCommands(StringBuilder buffer, IReadOnlyList<CommandSchema> subCommands)
        {
            if (!subCommands.Any())
                return;

            buffer.AppendLine("Commands:");

            foreach (var command in subCommands)
            {
                buffer.Append("  ");

                buffer.Append(command.Name);

                if (!command.Description.IsNullOrWhiteSpace())
                {
                    buffer.Append("  ");
                    buffer.Append(command.Description);
                }

                buffer.AppendLine();
            }

            buffer.AppendLine();
        }

        public string Build(ApplicationMetadata applicationMetadata,
            IReadOnlyList<CommandSchema> availableCommandSchemas,
            CommandSchema matchingCommandSchema)
        {
            var childCommandSchemas = GetChildCommandSchemas(availableCommandSchemas, matchingCommandSchema);

            var buffer = new StringBuilder();

            if (matchingCommandSchema.Name.IsNullOrWhiteSpace())
            {
                buffer.Append(applicationMetadata.Title);
                buffer.Append(" v");
                buffer.Append(applicationMetadata.VersionText);
                buffer.AppendLine().AppendLine();
            }

            AddDescription(buffer, matchingCommandSchema);
            AddUsage(buffer, applicationMetadata, matchingCommandSchema, childCommandSchemas);
            AddOptions(buffer, matchingCommandSchema);
            AddSubCommands(buffer, childCommandSchemas);

            if (matchingCommandSchema.Name.IsNullOrWhiteSpace() && childCommandSchemas.Any())
            {
                buffer.Append("You can run ");
                buffer.Append('`').Append(applicationMetadata.ExecutableName).Append(" [command] --help").Append('`');
                buffer.Append(" to show help on a specific command.");
                buffer.AppendLine();
            }

            return buffer.ToString().Trim();
        }
    }
}