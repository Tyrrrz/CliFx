using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    // TODO: add color
    public class CommandHelpTextBuilder : ICommandHelpTextBuilder
    {
        // TODO: move to context?
        // Entry assembly is null in tests
        private string GetExeName() => Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()?.Location) ?? "app";

        private void AddDescription(StringBuilder buffer, CommandSchema commands)
        {
            if (commands.Description.IsNullOrWhiteSpace())
                return;

            buffer.AppendLine("Description:");

            buffer.Append("  ");
            buffer.AppendLine(commands.Description);

            buffer.AppendLine();
        }

        private void AddUsage(StringBuilder buffer, CommandSchema command, IReadOnlyList<CommandSchema> subCommands)
        {
            buffer.AppendLine("Usage:");

            buffer.Append("  ");
            buffer.Append(GetExeName());

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

                buffer.Append(option.GetAliasesWithPrefixes().JoinToString("|"));

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
            if (command.IsDefault())
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

        public string Build(IReadOnlyList<CommandSchema> availableCommandSchemas, CommandSchema matchingCommandSchema)
        {
            var buffer = new StringBuilder();

            var subCommands = availableCommandSchemas.FindSubCommandSchemas(matchingCommandSchema.Name);

            AddDescription(buffer, matchingCommandSchema);
            AddUsage(buffer, matchingCommandSchema, subCommands);
            AddOptions(buffer, matchingCommandSchema);
            AddSubCommands(buffer, subCommands);

            if (matchingCommandSchema.IsDefault() && subCommands.Any())
            {
                buffer.Append("You can run ");
                buffer.Append('`').Append(GetExeName()).Append(" [command] --help").Append('`');
                buffer.Append(" to show help on a specific command.");
                buffer.AppendLine();
            }

            return buffer.ToString().Trim();
        }
    }
}