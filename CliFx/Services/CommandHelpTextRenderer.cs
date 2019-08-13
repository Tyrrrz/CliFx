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

        private bool _isEmpty = true;
        private int _position;

        public CommandHelpTextRenderer(IConsole console)
        {
            _console = console;
        }

        private void Render(string text)
        {
            _console.Output.Write(text);

            _isEmpty = false;
            _position += text.Length;
        }

        private void RenderNewLine()
        {
            _console.Output.WriteLine();

            _isEmpty = false;
            _position = 0;
        }

        private void RenderIndent(int spaces = 2) => Render(' '.Repeat(spaces));

        private void RenderColumnIndent(int spaces = 20, int margin = 2)
        {
            if (_position + margin >= spaces)
            {
                RenderNewLine();
                RenderIndent(spaces);
            }
            else
            {
                RenderIndent(spaces - _position);
            }
        }

        private void Render(string text, ConsoleColor foregroundColor) =>
            _console.WithForegroundColor(foregroundColor, () => Render(text));

        private void Render(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor) =>
            _console.WithColors(foregroundColor, backgroundColor, () => Render(text));

        private void RenderHeader(string text)
        {
            Render(text, ConsoleColor.Black, ConsoleColor.DarkGray);
            RenderNewLine();
        }

        private void RenderApplicationInfo(ApplicationMetadata applicationMetadata, CommandSchema commandSchema)
        {
            if (!commandSchema.IsDefault())
                return;

            // Margin
            if (!_isEmpty)
                RenderNewLine();

            // Title and version
            Render($"{applicationMetadata.Title} v{applicationMetadata.VersionText}", ConsoleColor.Yellow);
            RenderNewLine();
        }

        private void RenderDescription(CommandSchema commandSchema)
        {
            if (commandSchema.Description.IsNullOrWhiteSpace())
                return;

            // Margin
            if (!_isEmpty)
                RenderNewLine();

            // Header
            RenderHeader("Description");

            // Description
            RenderIndent();
            Render(commandSchema.Description);
            RenderNewLine();
        }

        private void RenderUsage(ApplicationMetadata applicationMetadata, CommandSchema commandSchema,
            IReadOnlyList<CommandSchema> childCommandSchemas)
        {
            // Margin
            if (!_isEmpty)
                RenderNewLine();

            // Header
            RenderHeader("Usage");

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
        }

        private void RenderOptions(CommandSchema commandSchema)
        {
            var options = new List<CommandOptionSchema>();
            options.AddRange(commandSchema.Options);

            options.Add(new CommandOptionSchema(null, "help", 'h', null, false, "Shows help text."));

            if (commandSchema.IsDefault())
                options.Add(new CommandOptionSchema(null, "version", null, null, false, "Shows application version."));

            // Margin
            if (!_isEmpty)
                RenderNewLine();

            // Header
            RenderHeader("Options");

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
                    RenderColumnIndent();
                    Render(option.Description);
                }

                RenderNewLine();
            }
        }

        private void RenderChildCommands(ApplicationMetadata applicationMetadata, CommandSchema commandSchema,
            IReadOnlyList<CommandSchema> childCommandSchemas)
        {
            if (!childCommandSchemas.Any())
                return;

            // Margin
            if (!_isEmpty)
                RenderNewLine();

            // Header
            RenderHeader("Commands");

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
                    RenderColumnIndent();
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

            RenderApplicationInfo(applicationMetadata, matchingCommandSchema);
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