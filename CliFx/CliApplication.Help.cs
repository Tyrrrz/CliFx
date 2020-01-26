using System;
using System.Linq;
using CliFx.Domain;
using CliFx.Internal;

namespace CliFx
{
    public partial class CliApplication
    {
        private void RenderHelp(ApplicationSchema applicationSchema, CommandSchema commandSchema)
        {
            // Track position
            var column = 0;
            var row = 0;

            // Get child commands
            var childCommands = applicationSchema.GetChildCommands(commandSchema.Name);

            // Define helper functions

            bool IsEmpty() => column == 0 && row == 0;

            void Render(string text)
            {
                _console.Output.Write(text);

                column += text.Length;
            }

            void RenderNewLine()
            {
                _console.Output.WriteLine();

                column = 0;
                row++;
            }

            void RenderMargin(int lines = 1)
            {
                if (!IsEmpty())
                {
                    for (var i = 0; i < lines; i++)
                        RenderNewLine();
                }
            }

            void RenderIndent(int spaces = 2)
            {
                Render(' '.Repeat(spaces));
            }

            void RenderColumnIndent(int spaces = 20, int margin = 2)
            {
                if (column + margin >= spaces)
                {
                    RenderNewLine();
                    RenderIndent(spaces);
                }
                else
                {
                    RenderIndent(spaces - column);
                }
            }

            void RenderWithColor(string text, ConsoleColor foregroundColor)
            {
                _console.WithForegroundColor(foregroundColor, () => Render(text));
            }

            void RenderWithColors(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
            {
                _console.WithColors(foregroundColor, backgroundColor, () => Render(text));
            }

            void RenderHeader(string text)
            {
                RenderWithColors(text, ConsoleColor.Black, ConsoleColor.DarkGray);
                RenderNewLine();
            }

            void RenderApplicationInfo()
            {
                if (!commandSchema.IsDefault)
                    return;

                // Title and version
                RenderWithColor(_metadata.Title, ConsoleColor.Yellow);
                Render(" ");
                RenderWithColor(_metadata.VersionText, ConsoleColor.Yellow);
                RenderNewLine();

                // Description
                if (!string.IsNullOrWhiteSpace(_metadata.Description))
                {
                    Render(_metadata.Description);
                    RenderNewLine();
                }
            }

            void RenderDescription()
            {
                if (string.IsNullOrWhiteSpace(commandSchema.Description))
                    return;

                // Margin
                RenderMargin();

                // Header
                RenderHeader("Description");

                // Description
                RenderIndent();
                Render(commandSchema.Description!);
                RenderNewLine();
            }

            void RenderUsage()
            {
                // Margin
                RenderMargin();

                // Header
                RenderHeader("Usage");

                // Exe name
                RenderIndent();
                Render(_metadata.ExecutableName);

                // Command name
                if (!string.IsNullOrWhiteSpace(commandSchema.Name))
                {
                    Render(" ");
                    RenderWithColor(commandSchema.Name!, ConsoleColor.Cyan);
                }

                // Child command
                if (childCommands.Any())
                {
                    Render(" ");
                    RenderWithColor("[command]", ConsoleColor.Cyan);
                }

                // Arguments
                foreach (var parameter in commandSchema.Parameters)
                {
                    Render(" ");
                    Render($"<{parameter.DisplayName}>");
                }

                // Required options
                var requiredOptionSchemas = commandSchema.Options
                    .Where(o => o.IsRequired)
                    .ToArray();

                foreach (var requiredOption in requiredOptionSchemas)
                {
                    Render($" --{requiredOption.Name} <value>");
                }

                // Options placeholder
                var notRequiredOrDefaultOptionCount = commandSchema.Options.Count(o => !o.IsRequired);

                if (notRequiredOrDefaultOptionCount > 0)
                {
                    Render(" ");
                    RenderWithColor("[options]", ConsoleColor.White);
                }

                RenderNewLine();
            }

            void RenderParameters()
            {
                // Do not render anything if the command has no parameters
                if (!commandSchema.Parameters.Any())
                    return;

                // Margin
                RenderMargin();

                // Header
                RenderHeader("Arguments");

                // Order parameters
                var orderedArgumentSchemas = commandSchema.Parameters
                    .OrderBy(p => p.Order)
                    .ToArray();

                // Arguments
                foreach (var argumentSchema in orderedArgumentSchemas)
                {
                    // Is required
                    RenderWithColor("* ", ConsoleColor.Red);

                    // Short name
                    RenderWithColor($"{argumentSchema.DisplayName}", ConsoleColor.White);

                    // Description
                    if (!string.IsNullOrWhiteSpace(argumentSchema.Description))
                    {
                        RenderColumnIndent();
                        Render(argumentSchema.Description!);
                    }

                    RenderNewLine();
                }
            }

            void RenderOptions()
            {
                // Margin
                RenderMargin();

                // Header
                RenderHeader("Options");

                // Order options and append built-in options
                var allOptionSchemas = commandSchema.Options
                    .OrderByDescending(o => o.IsRequired)
                    .ToList();

                allOptionSchemas.Add(new CommandOptionSchema(null!, "help", 'h', null, false, "Shows help text."));

                if (commandSchema.IsDefault)
                    allOptionSchemas.Add(new CommandOptionSchema(null!, "version", null, null, false, "Shows version information."));

                // Options
                foreach (var optionSchema in allOptionSchemas)
                {
                    // Is required
                    if (optionSchema.IsRequired)
                    {
                        RenderWithColor("* ", ConsoleColor.Red);
                    }
                    else
                    {
                        RenderIndent();
                    }

                    // Short name
                    if (optionSchema.ShortName != null)
                    {
                        RenderWithColor($"-{optionSchema.ShortName}", ConsoleColor.White);
                    }

                    // Delimiter
                    if (!string.IsNullOrWhiteSpace(optionSchema.Name) && optionSchema.ShortName != null)
                    {
                        Render("|");
                    }

                    // Name
                    if (!string.IsNullOrWhiteSpace(optionSchema.Name))
                    {
                        RenderWithColor($"--{optionSchema.Name}", ConsoleColor.White);
                    }

                    // Description
                    if (!string.IsNullOrWhiteSpace(optionSchema.Description))
                    {
                        RenderColumnIndent();
                        Render(optionSchema.Description!);
                    }

                    RenderNewLine();
                }
            }

            void RenderChildCommands()
            {
                if (!childCommands.Any())
                    return;

                // Margin
                RenderMargin();

                // Header
                RenderHeader("Commands");

                // Child commands
                foreach (var childCommandSchema in childCommands)
                {
                    var relativeCommandName =
                        string.IsNullOrWhiteSpace(childCommandSchema.Name) || string.IsNullOrWhiteSpace(commandSchema.Name)
                            ? childCommandSchema.Name
                            : childCommandSchema.Name.Substring(commandSchema.Name.Length + 1);

                    // Name
                    RenderIndent();
                    RenderWithColor(relativeCommandName, ConsoleColor.Cyan);

                    // Description
                    if (!string.IsNullOrWhiteSpace(childCommandSchema.Description))
                    {
                        RenderColumnIndent();
                        Render(childCommandSchema.Description!);
                    }

                    RenderNewLine();
                }

                // Margin
                RenderMargin();

                // Child command help tip
                Render("You can run `");
                Render(_metadata.ExecutableName);

                if (!string.IsNullOrWhiteSpace(commandSchema.Name))
                {
                    Render(" ");
                    RenderWithColor(commandSchema.Name, ConsoleColor.Cyan);
                }

                Render(" ");
                RenderWithColor("[command]", ConsoleColor.Cyan);

                Render(" ");
                RenderWithColor("--help", ConsoleColor.White);

                Render("` to show help on a specific command.");

                RenderNewLine();
            }

            // Reset color just in case
            _console.ResetColor();

            // Render everything
            RenderApplicationInfo();
            RenderDescription();
            RenderUsage();
            RenderParameters();
            RenderOptions();
            RenderChildCommands();
        }
    }
}