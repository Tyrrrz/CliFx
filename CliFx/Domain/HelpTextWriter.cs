using System;
using System.Linq;
using CliFx.Internal;

namespace CliFx.Domain
{
    internal class HelpTextWriter
    {
        private readonly ApplicationMetadata _metadata;
        private readonly IConsole _console;

        public HelpTextWriter(ApplicationMetadata metadata, IConsole console)
        {
            _metadata = metadata;
            _console = console;
        }

        public void Write(ApplicationSchema applicationSchema, CommandSchema command)
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
                    RenderIndent(margin);
                }
            }

            void RenderWithColor(string text, ConsoleColor foregroundColor)
            {
                _console.WithForegroundColor(foregroundColor, () => Render(text));
            }

            void RenderHeader(string text)
            {
                RenderWithColor(text, ConsoleColor.Magenta);
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
                    Render(parameter.IsScalar
                        ? $"<{parameter.DisplayName}>"
                        : $"<{parameter.DisplayName}...>");
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
                        Render(option.IsScalar
                            ? "<value>"
                            : "<values...>");
                    }
                    else
                    {
                        RenderWithColor($"-{option.ShortName}", ConsoleColor.White);
                        Render(" ");
                        Render(option.IsScalar
                            ? "<value>"
                            : "<values...>");
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
                        Render(" ");
                    }

                    // Valid values
                    var validValues = parameter.GetValidValues();
                    if (validValues.Any())
                    {
                        Render($"Valid values: {string.Join(", ", validValues)}.");
                        Render(" ");
                    }

                    RenderNewLine();
                }
            }

            void RenderOptions()
            {
                RenderMargin();
                RenderHeader("Options");

                // Instantiate a temporary instance of the command so we can get default values from it.
                var tempInstance = command.Type is null ? null : Activator.CreateInstance(command.Type);

                var options = command.Options
                    .OrderByDescending(o => o.IsRequired)
                    .Concat(command.GetBuiltInOptions())
                    .ToArray();

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

                    // Valid values
                    var validValues = option.GetValidValues();
                    if (validValues.Any())
                    {
                        Render($"Valid values: {string.Join(", ", validValues)}.");
                        Render(" ");
                    }

                    // Environment variable
                    if (!string.IsNullOrWhiteSpace(option.EnvironmentVariableName))
                    {
                        Render($"Environment variable: {option.EnvironmentVariableName}");
                        Render(" ");
                    }

                    // Default value
                    if (!option.IsRequired)
                    {
                        var defaultValue = option.GetDefaultValue(tempInstance);
                        // Don't print the default value if it's null.
                        if (defaultValue is object)
                        {
                            Render($"(Default: {defaultValue})");
                            Render(" ");
                        }
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
                        !string.IsNullOrWhiteSpace(command.Name)
                            ? childCommand.Name!.Substring(command.Name.Length + 1)
                            : childCommand.Name!;

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