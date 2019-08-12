using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    public partial class CommandHelpTextRenderer : ICommandHelpTextRenderer
    {
        private readonly IConsole _console;

        private int _position;

        public CommandHelpTextRenderer(IConsole console)
        {
            _console = console;
        }

        private void Render(string text)
        {
            _console.Output.Write(text);
            _position += text.Length;
        }

        private void RenderNewLine()
        {
            _console.Output.WriteLine();
            _position = 0;
        }

        private void RenderIndent(int spaces = 2) => Render(' '.Repeat(spaces));

        private void Render(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor = ConsoleColor.Black) =>
            _console.WithColor(foregroundColor, backgroundColor, () => Render(text));

        private void RenderDescription(CommandSchema commandSchema)
        {
            if (commandSchema.Description.IsNullOrWhiteSpace())
                return;

            // Header
            Render("Description", ConsoleColor.Black, ConsoleColor.DarkGray);
            RenderNewLine();

            // Description
            RenderIndent();
            Render(commandSchema.Description);
            RenderNewLine();

            // Margin
            RenderNewLine();
        }

        private void RenderUsage(ApplicationMetadata applicationMetadata, CommandSchema commandSchema,
            IReadOnlyList<CommandSchema> childCommandSchemas)
        {
            // Header
            Render("Usage", ConsoleColor.Black, ConsoleColor.DarkGray);
            RenderNewLine();

            // Exe name
            RenderIndent();
            Render(applicationMetadata.ExecutableName);

            // Command name
            if (!commandSchema.IsDefault())
            {
                Render(" ");
                Render(commandSchema.Name, ConsoleColor.Cyan);
            }

            // Child command
            if (childCommandSchemas.Any())
            {
                Render(" ");
                Render("[command]", ConsoleColor.Cyan);
            }

            // Options
            Render(" ");
            Render("[options]", ConsoleColor.White);
            RenderNewLine();

            // Margin
            RenderNewLine();
        }

        private void RenderOptions(CommandSchema commandSchema)
        {
            var options = new List<CommandOptionSchema>();
            options.AddRange(commandSchema.Options);

            options.Add(new CommandOptionSchema(null, "help", 'h', null, false, "Shows help text."));

            if (commandSchema.IsDefault())
                options.Add(new CommandOptionSchema(null, "version", null, null, false, "Shows application version."));

            // Header
            Render("Options", ConsoleColor.Black, ConsoleColor.DarkGray);
            RenderNewLine();

            // Options
            foreach (var option in options)
            {
                RenderIndent();

                // Short name
                if (option.ShortName != null)
                {
                    Render($"-{option.ShortName}", ConsoleColor.White);
                }

                // Delimiter
                if (!option.Name.IsNullOrWhiteSpace() && option.ShortName != null)
                {
                    Render("|");
                }

                // Name
                if (!option.Name.IsNullOrWhiteSpace())
                {
                    Render($"--{option.Name}", ConsoleColor.White);
                }

                // Description
                if (!option.Description.IsNullOrWhiteSpace())
                {
                    const int threshold = 20;

                    if (_position >= threshold)
                    {
                        RenderNewLine();
                        RenderIndent(threshold);
                    }
                    else
                    {
                        RenderIndent(threshold - _position);
                    }

                    Render(option.Description);
                }

                RenderNewLine();
            }

            RenderNewLine();
        }

        private void RenderChildCommands(ApplicationMetadata applicationMetadata, CommandSchema commandSchema,
            IReadOnlyList<CommandSchema> childCommandSchemas)
        {
            if (!childCommandSchemas.Any())
                return;

            // Header
            Render("Commands", ConsoleColor.Black, ConsoleColor.DarkGray);
            RenderNewLine();

            // Child commands
            foreach (var childCommandSchema in childCommandSchemas)
            {
                var relativeCommandName = GetRelativeCommandName(childCommandSchema, commandSchema);

                // Name
                RenderIndent();
                Render(relativeCommandName, ConsoleColor.Cyan);

                // Description
                if (!childCommandSchema.Description.IsNullOrWhiteSpace())
                {
                    const int threshold = 20;

                    if (_position >= threshold)
                    {
                        RenderNewLine();
                        RenderIndent(threshold);
                    }
                    else
                    {
                        RenderIndent(threshold - _position);
                    }

                    Render(childCommandSchema.Description);
                }

                RenderNewLine();
            }

            // Margin
            RenderNewLine();

            // Child command help tip
            Render("You can run `");
            Render(applicationMetadata.ExecutableName);

            if (!commandSchema.IsDefault())
            {
                Render(" ");
                Render(commandSchema.Name, ConsoleColor.Cyan);
            }

            Render(" ");
            Render("[command]", ConsoleColor.Cyan);

            Render(" ");
            Render("--help", ConsoleColor.White);

            Render("` to show help on a specific command.");

            RenderNewLine();
        }

        public void RenderHelpText(ApplicationMetadata applicationMetadata,
            IReadOnlyList<CommandSchema> availableCommandSchemas, CommandSchema matchingCommandSchema)
        {
            var childCommandSchemas = availableCommandSchemas
                .Where(c => availableCommandSchemas.FindParent(c.Name) == matchingCommandSchema)
                .ToArray();

            // Render application info
            if (matchingCommandSchema.IsDefault())
            {
                Render($"{applicationMetadata.Title} v{applicationMetadata.VersionText}", ConsoleColor.Yellow);
                RenderNewLine();
                RenderNewLine();
            }

            RenderDescription(matchingCommandSchema);
            RenderUsage(applicationMetadata, matchingCommandSchema, childCommandSchemas);
            RenderOptions(matchingCommandSchema);
            RenderChildCommands(applicationMetadata, matchingCommandSchema, childCommandSchemas);
        }
    }

    public partial class CommandHelpTextRenderer
    {
        private static string GetRelativeCommandName(CommandSchema commandSchema, CommandSchema parentCommandSchema) =>
            parentCommandSchema.Name.IsNullOrWhiteSpace()
                ? commandSchema.Name
                : commandSchema.Name.Substring(parentCommandSchema.Name.Length + 1);
    }
}