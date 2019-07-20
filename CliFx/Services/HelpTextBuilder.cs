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
    public class HelpTextBuilder : IHelpTextBuilder
    {
        // TODO: move to context?
        private string GetExeName() => Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()?.Location);

        // TODO: move to context?
        private string GetVersionText() => Assembly.GetEntryAssembly()?.GetName().Version.ToString();

        private IReadOnlyList<string> GetOptionIdentifiers(CommandOptionSchema option)
        {
            var result = new List<string>();

            if (option.ShortName != null)
                result.Add("-" + option.ShortName.Value);

            if (!option.Name.IsNullOrWhiteSpace())
                result.Add("--" + option.Name);

            return result;
        }

        private void AddDescription(StringBuilder buffer, CommandContext context)
        {
            if (context.CommandSchema.Description.IsNullOrWhiteSpace())
                return;

            buffer.AppendLine("Description:");

            buffer.Append("    ");
            buffer.AppendLine(context.CommandSchema.Description);

            buffer.AppendLine();
        }

        private void AddUsage(StringBuilder buffer, CommandContext context)
        {
            buffer.AppendLine("Usage:");

            buffer.Append("    ");
            buffer.Append(GetExeName());

            if (!context.CommandSchema.Name.IsNullOrWhiteSpace())
            {
                buffer.Append(' ');
                buffer.Append(context.CommandSchema.Name);
            }

            if (context.CommandSchema.Options.Any())
            {
                buffer.Append(' ');
                buffer.Append("[options]");
            }

            buffer.AppendLine().AppendLine();
        }

        private void AddOptions(StringBuilder buffer, CommandContext context)
        {
            if (!context.CommandSchema.Options.Any())
                return;

            buffer.AppendLine("Options:");

            foreach (var option in context.CommandSchema.Options)
            {
                buffer.Append(option.IsRequired ? "  * " : "    ");

                buffer.Append(GetOptionIdentifiers(option).JoinToString("|"));

                if (!option.Description.IsNullOrWhiteSpace())
                {
                    buffer.Append("  ");
                    buffer.Append(option.Description);
                }

                buffer.AppendLine();
            }

            buffer.AppendLine();
        }

        public string Build(CommandContext context)
        {
            var buffer = new StringBuilder();

            AddDescription(buffer, context);
            AddUsage(buffer, context);
            AddOptions(buffer, context);

            // TODO: add default command help

            return buffer.ToString();
        }
    }
}