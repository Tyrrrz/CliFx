using System;
using System.Linq;
using CliFx.Domain;
using CliFx.Internal;

namespace CliFx
{
    public partial class CliApplication
    {
        private void RenderHelp(ApplicationSchema applicationSchema, CommandSchema command)
        {
            var column = 0;
            var row = 0;

            var childCommands = applicationSchema.GetChildCommands(command.Name);

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
                if (column + margin < spaces)
                {
                    RenderIndent(spaces - column);
                }
                else
                {
                    Render(" ");
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
                if (!command.IsDefault)
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
                if (string.IsNullOrWhiteSpace(command.Description))
                    return;

                RenderMargin();
                RenderHeader("Description");

                RenderIndent();
                Render(command.Description);
                RenderNewLine();
            }

            void RenderUsage()
            {
                RenderMargin();
                RenderHeader("Usage");

                // Exe name
                RenderIndent();
                Render(_metadata.ExecutableName);

                // Command name
                if (!string.IsNullOrWhiteSpace(command.Name))
                {
                    Render(" ");
                    RenderWithColor(command.Name, ConsoleColor.Cyan);
                }

                // Child command placeholder
                if (childCommands.Any())
                {
                    Render(" ");
                    RenderWithColor("[command]", ConsoleColor.Cyan);
                }

                // Parameters
                foreach (var parameter in command.Parameters)
                {
                    Render(" ");
                    Render($"<{parameter.DisplayName}>");
                }

                // Required options
                var requiredOptionSchemas = command.Options
                    .Where(o => o.IsRequired)
                    .ToArray();

                foreach (var option in requiredOptionSchemas)
                {
                    Render(" ");
                    if (!string.IsNullOrWhiteSpace(option.Name))
                    {
                        RenderWithColor($"--{option.Name}", ConsoleColor.White);
                        Render(" ");
                        Render("<value>");
                    }
                    else
                    {
                        RenderWithColor($"-{option.ShortName} <value>", ConsoleColor.White);
                        Render(" ");
                        Render("<value>");
                    }
                }

                // Options placeholder
                if (command.Options.Count != requiredOptionSchemas.Length)
                {
                    Render(" ");
                    RenderWithColor("[options]", ConsoleColor.White);
                }

                RenderNewLine();
            }

            void RenderParameters()
            {
                if (!command.Parameters.Any())
                    return;

                RenderMargin();
                RenderHeader("Parameters");

                var parameters = command.Parameters
                    .OrderBy(p => p.Order)
                    .ToArray();

                foreach (var parameter in parameters)
                {
                    RenderWithColor("* ", ConsoleColor.Red);
                    RenderWithColor($"{parameter.DisplayName}", ConsoleColor.White);

                    RenderColumnIndent();

                    // Description
                    if (!string.IsNullOrWhiteSpace(parameter.Description))
                    {
                        Render(parameter.Description);
                    }

                    RenderNewLine();
                }
            }

            void RenderOptions()
            {
                RenderMargin();
                RenderHeader("Options");

                var options = command.Options
                    .OrderByDescending(o => o.IsRequired)
                    .ToList();

                // Add built-in options
                options.Add(CommandOptionSchema.HelpOption);
                if (command.IsDefault)
                    options.Add(CommandOptionSchema.VersionOption);

                foreach (var option in options)
                {
                    if (option.IsRequired)
                    {
                        RenderWithColor("* ", ConsoleColor.Red);
                    }
                    else
                    {
                        RenderIndent();
                    }

                    // Short name
                    if (option.ShortName != null)
                    {
                        RenderWithColor($"-{option.ShortName}", ConsoleColor.White);
                    }

                    // Delimiter
                    if (!string.IsNullOrWhiteSpace(option.Name) && option.ShortName != null)
                    {
                        Render("|");
                    }

                    // Name
                    if (!string.IsNullOrWhiteSpace(option.Name))
                    {
                        RenderWithColor($"--{option.Name}", ConsoleColor.White);
                    }

                    RenderColumnIndent();

                    // Description
                    if (!string.IsNullOrWhiteSpace(option.Description))
                    {
                        Render(option.Description);
                        Render(" ");
                    }

                    // Environment variable
                    if (!string.IsNullOrWhiteSpace(option.EnvironmentVariableName))
                    {
                        Render($"(Environment variable: {option.EnvironmentVariableName}).");
                        Render(" ");
                    }

                    RenderNewLine();
                }
            }

            void RenderChildCommands()
            {
                if (!childCommands.Any())
                    return;

                RenderMargin();
                RenderHeader("Commands");

                foreach (var childCommand in childCommands)
                {
                    var relativeCommandName =
                        string.IsNullOrWhiteSpace(childCommand.Name) || string.IsNullOrWhiteSpace(command.Name)
                            ? childCommand.Name
                            : childCommand.Name.Substring(command.Name.Length + 1);

                    // Name
                    RenderIndent();
                    RenderWithColor(relativeCommandName, ConsoleColor.Cyan);

                    // Description
                    if (!string.IsNullOrWhiteSpace(childCommand.Description))
                    {
                        RenderColumnIndent();
                        Render(childCommand.Description);
                    }

                    RenderNewLine();
                }

                RenderMargin();

                // Child command help tip
                Render("You can run `");
                Render(_metadata.ExecutableName);

                if (!string.IsNullOrWhiteSpace(command.Name))
                {
                    Render(" ");
                    RenderWithColor(command.Name, ConsoleColor.Cyan);
                }

                Render(" ");
                RenderWithColor("[command]", ConsoleColor.Cyan);

                Render(" ");
                RenderWithColor("--help", ConsoleColor.White);

                Render("` to show help on a specific command.");

                RenderNewLine();
            }

            _console.ResetColor();
            RenderApplicationInfo();
            RenderDescription();
            RenderUsage();
            RenderParameters();
            RenderOptions();
            RenderChildCommands();
        }
    }
}