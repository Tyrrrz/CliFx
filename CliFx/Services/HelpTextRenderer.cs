using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    /// <summary>
    /// Default implementation of <see cref="IHelpTextRenderer"/>.
    /// </summary>
    public partial class HelpTextRenderer : IHelpTextRenderer
    {
        /// <inheritdoc />
        public void RenderHelpText(IConsole console, HelpTextSource source)
        {
            // Track position
            var column = 0;
            var row = 0;

            // Get built-in option schemas (help and version)
            var builtInOptionSchemas = new List<CommandOptionSchema> { CommandOptionSchema.HelpOption };
            if (source.TargetCommandSchema.IsDefault())
                builtInOptionSchemas.Add(CommandOptionSchema.VersionOption);

            // Get child command schemas
            var childCommandSchemas = source.AvailableCommandSchemas
                .Where(c => source.AvailableCommandSchemas.FindParent(c.Name) == source.TargetCommandSchema)
                .ToArray();

            // Define helper functions

            bool IsEmpty() => column == 0 && row == 0;

            void Render(string text)
            {
                console.Output.Write(text);

                column += text.Length;
            }

            void RenderNewLine()
            {
                console.Output.WriteLine();

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
                console.WithForegroundColor(foregroundColor, () => Render(text));
            }

            void RenderWithColors(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
            {
                console.WithColors(foregroundColor, backgroundColor, () => Render(text));
            }

            void RenderHeader(string text)
            {
                RenderWithColors(text, ConsoleColor.Black, ConsoleColor.DarkGray);
                RenderNewLine();
            }

            void RenderApplicationInfo()
            {
                if (!source.TargetCommandSchema.IsDefault())
                    return;

                // Title and version
                RenderWithColor(source.ApplicationMetadata.Title, ConsoleColor.Yellow);
                Render(" ");
                RenderWithColor(source.ApplicationMetadata.VersionText, ConsoleColor.Yellow);
                RenderNewLine();

                // Description
                if (!string.IsNullOrWhiteSpace(source.ApplicationMetadata.Description))
                {
                    Render(source.ApplicationMetadata.Description!);
                    RenderNewLine();
                }
            }

            void RenderDescription()
            {
                if (string.IsNullOrWhiteSpace(source.TargetCommandSchema.Description))
                    return;

                // Margin
                RenderMargin();

                // Header
                RenderHeader("Description");

                // Description
                RenderIndent();
                Render(source.TargetCommandSchema.Description!);
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
                Render(source.ApplicationMetadata.ExecutableName);

                // Command name
                if (!string.IsNullOrWhiteSpace(source.TargetCommandSchema.Name))
                {
                    Render(" ");
                    RenderWithColor(source.TargetCommandSchema.Name!, ConsoleColor.Cyan);
                }

                // Child command
                if (childCommandSchemas.Any())
                {
                    Render(" ");
                    RenderWithColor("[command]", ConsoleColor.Cyan);
                }

                // Arguments
                foreach (var argumentSchema in source.TargetCommandSchema.Arguments)
                {
                    Render(" ");
                    RenderWithColor(argumentSchema.ToString(), ConsoleColor.White);
                }

                // Options
                Render(" ");
                RenderWithColor("[options]", ConsoleColor.White);
                RenderNewLine();
            }

            void RenderArguments()
            {
                // Do not render anything if the command has no arguments
                if (source.TargetCommandSchema.Arguments.Count == 0)
                    return;

                // Margin
                RenderMargin();

                // Header
                RenderHeader("Arguments");

                // Order arguments
                var orderedArgumentSchemas = source.TargetCommandSchema.Arguments
                    .Ordered()
                    .ToArray();

                // Arguments
                foreach (var argumentSchema in orderedArgumentSchemas)
                {
                    // Is required
                    if (argumentSchema.IsRequired)
                    {
                        RenderWithColor("* ", ConsoleColor.Red);
                    }
                    else
                    {
                        RenderIndent();
                    }

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
                var allOptionSchemas = source.TargetCommandSchema.Options
                    .OrderByDescending(o => o.IsRequired)
                    .Concat(builtInOptionSchemas)
                    .ToArray();

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
                if (!childCommandSchemas.Any())
                    return;

                // Margin
                RenderMargin();

                // Header
                RenderHeader("Commands");

                // Child commands
                foreach (var childCommandSchema in childCommandSchemas)
                {
                    var relativeCommandName = GetRelativeCommandName(childCommandSchema, source.TargetCommandSchema)!;

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
                Render(source.ApplicationMetadata.ExecutableName);

                if (!string.IsNullOrWhiteSpace(source.TargetCommandSchema.Name))
                {
                    Render(" ");
                    RenderWithColor(source.TargetCommandSchema.Name, ConsoleColor.Cyan);
                }

                Render(" ");
                RenderWithColor("[command]", ConsoleColor.Cyan);

                Render(" ");
                RenderWithColor("--help", ConsoleColor.White);

                Render("` to show help on a specific command.");

                RenderNewLine();
            }

            // Reset color just in case
            console.ResetColor();

            // Render everything
            RenderApplicationInfo();
            RenderDescription();
            RenderUsage();
            RenderArguments();
            RenderOptions();
            RenderChildCommands();
        }
    }

    public partial class HelpTextRenderer
    {
        private static string? GetRelativeCommandName(CommandSchema commandSchema, CommandSchema parentCommandSchema) =>
            string.IsNullOrWhiteSpace(parentCommandSchema.Name) || string.IsNullOrWhiteSpace(commandSchema.Name)
                ? commandSchema.Name
                : commandSchema.Name!.Substring(parentCommandSchema.Name!.Length + 1);
    }
}