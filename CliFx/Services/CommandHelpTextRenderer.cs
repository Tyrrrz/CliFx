using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Default implementation of <see cref="ICommandHelpTextRenderer"/>.
    /// </summary>
    public partial class CommandHelpTextRenderer : ICommandHelpTextRenderer
    {
        /// <inheritdoc />
        public void RenderHelpText(IConsole console, HelpTextSource source) => new Impl(console, source).RenderHelpText();
    }

    public partial class CommandHelpTextRenderer
    {
        private class Impl
        {
            private readonly IConsole _console;
            private readonly HelpTextSource _source;

            private bool _isEmpty = true;
            private int _position;

            public Impl(IConsole console, HelpTextSource source)
            {
                _console = console;
                _source = source;
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

            private void RenderApplicationInfo()
            {
                if (!_source.TargetCommandSchema.IsDefault())
                    return;

                // Margin
                if (!_isEmpty)
                    RenderNewLine();

                // Title
                Render(_source.ApplicationMetadata.Title, ConsoleColor.Yellow);
                Render(" ");
                Render(_source.ApplicationMetadata.VersionText, ConsoleColor.Yellow);
                RenderNewLine();
            }

            private void RenderDescription()
            {
                if (_source.TargetCommandSchema.Description.IsNullOrWhiteSpace())
                    return;

                // Margin
                if (!_isEmpty)
                    RenderNewLine();

                // Header
                RenderHeader("Description");

                // Description
                RenderIndent();
                Render(_source.TargetCommandSchema.Description);
                RenderNewLine();
            }

            private void RenderUsage()
            {
                // Margin
                if (!_isEmpty)
                    RenderNewLine();

                // Header
                RenderHeader("Usage");

                // Exe name
                RenderIndent();
                Render(_source.ApplicationMetadata.ExecutableName);

                // Command name
                if (!_source.TargetCommandSchema.IsDefault())
                {
                    Render(" ");
                    Render(_source.TargetCommandSchema.Name, ConsoleColor.Cyan);
                }

                // Child command
                if (GetChildCommandSchemas(_source.AvailableCommandSchemas, _source.TargetCommandSchema).Any())
                {
                    Render(" ");
                    Render("[command]", ConsoleColor.Cyan);
                }

                // Options
                Render(" ");
                Render("[options]", ConsoleColor.White);
                RenderNewLine();
            }

            private void RenderOptions()
            {
                var options = new List<CommandOptionSchema>();
                options.AddRange(_source.TargetCommandSchema.Options);

                options.Add(new CommandOptionSchema(null, "help", 'h', null, false, "Shows help text."));

                if (_source.TargetCommandSchema.IsDefault())
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

            private void RenderChildCommands()
            {
                var childCommandSchemas = GetChildCommandSchemas(_source.AvailableCommandSchemas, _source.TargetCommandSchema).ToArray();

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
                    var relativeCommandName = GetRelativeCommandName(childCommandSchema, _source.TargetCommandSchema);

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
                Render(_source.ApplicationMetadata.ExecutableName);

                if (!_source.TargetCommandSchema.IsDefault())
                {
                    Render(" ");
                    Render(_source.TargetCommandSchema.Name, ConsoleColor.Cyan);
                }

                Render(" ");
                Render("[command]", ConsoleColor.Cyan);

                Render(" ");
                Render("--help", ConsoleColor.White);

                Render("` to show help on a specific command.");

                RenderNewLine();
            }

            public void RenderHelpText()
            {
                RenderApplicationInfo();
                RenderDescription();
                RenderUsage();
                RenderOptions();
                RenderChildCommands();
            }
        }
    }

    public partial class CommandHelpTextRenderer
    {
        private static IEnumerable<CommandSchema> GetChildCommandSchemas(IReadOnlyList<CommandSchema> availableCommandSchemas,
            CommandSchema parentCommandSchema) =>
            availableCommandSchemas.Where(c => availableCommandSchemas.FindParent(c.Name) == parentCommandSchema);

        private static string GetRelativeCommandName(CommandSchema commandSchema, CommandSchema parentCommandSchema) =>
            parentCommandSchema.Name.IsNullOrWhiteSpace()
                ? commandSchema.Name
                : commandSchema.Name.Substring(parentCommandSchema.Name.Length + 1);
    }
}