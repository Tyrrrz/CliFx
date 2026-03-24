using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CliFx.Binding;
using CliFx.Infrastructure;
using CliFx.Utils.Extensions;

namespace CliFx.Help;

internal class HelpWriter(HelpContext context, ConsoleWriter consoleWriter)
    : FormattedConsoleWriter(consoleWriter)
{
    private void WriteHeader(string text)
    {
        Write(ConsoleColor.White, text.ToUpperInvariant());
        WriteLine();
    }

    private void WriteCommandInvocation()
    {
        Write(context.Metadata.ExecutableName);

        // Command name
        if (!string.IsNullOrWhiteSpace(context.Command.Name))
        {
            Write(' ');
            Write(ConsoleColor.Cyan, context.Command.Name);
        }
    }

    private void WriteCommandUsage()
    {
        if (!IsEmpty)
            WriteVerticalMargin();

        WriteHeader("Usage");

        // Current command
        {
            WriteHorizontalMargin();

            WriteCommandInvocation();
            Write(' ');

            // Parameters
            foreach (var parameter in context.Command.Parameters.OrderBy(p => p.Order))
            {
                Write(
                    ConsoleColor.DarkCyan,
                    !parameter.Converter.CanConvertSequence
                        ? $"<{parameter.Name}>"
                        : $"<{parameter.Name}...>"
                );
                Write(' ');
            }

            // Required options
            foreach (var option in context.Command.Options.Where(o => o.IsRequired))
            {
                Write(
                    ConsoleColor.Yellow,
                    !string.IsNullOrWhiteSpace(option.Name)
                        ? $"--{option.Name}"
                        : $"-{option.ShortName}"
                );
                Write(' ');

                Write(
                    ConsoleColor.White,
                    !option.Converter.CanConvertSequence ? "<value>" : "<values...>"
                );
                Write(' ');
            }

            // Placeholder for non-required options
            if (context.Command.Options.Any(o => !o.IsRequired))
            {
                Write(ConsoleColor.Yellow, "[options]");
            }

            WriteLine();
        }

        // Child commands
        if (context.Root.GetChildCommands(context.Command.Name).Any())
        {
            WriteHorizontalMargin();

            WriteCommandInvocation();
            Write(' ');

            // Placeholder for child command
            Write(ConsoleColor.Cyan, "[command]");
            Write(' ');

            // Placeholder for other arguments
            Write("[...]");

            WriteLine();
        }
    }

    private void WriteCommandDescription()
    {
        if (string.IsNullOrWhiteSpace(context.Command.Description))
            return;

        if (!IsEmpty)
            WriteVerticalMargin();

        WriteHeader("Description");

        WriteHorizontalMargin();

        Write(context.Command.Description);
        WriteLine();
    }

    private void WriteCommandInputDefaultValue(CommandInputDescriptor input)
    {
        var defaultValue = context.CommandDefaultValues.GetValueOrDefault(input);
        if (defaultValue is null)
            return;

        // Normalize to an array to process both scalar and sequence default values uniformly
        var defaultValues =
            defaultValue is not string && defaultValue is IEnumerable defaultValueAsEnumerable
                ? defaultValueAsEnumerable.Cast<object>().ToArray()
                : [defaultValue];

        // Only strings, chars, bools, and types that implement IFormattable have
        // meaningful ToString() representations.
        if (!defaultValues.All(v => v is string or char or bool or IFormattable))
            return;

        var isFirst = true;

        foreach (var value in defaultValues)
        {
            if (value is not IFormattable and not IConvertible)
                continue;

            if (isFirst)
            {
                Write(ConsoleColor.White, "Default: ");
                isFirst = false;
            }
            else
            {
                Write(", ");
            }

            Write('"');
            Write(value.ToString(CultureInfo.InvariantCulture));
            Write('"');
        }

        if (!isFirst)
            Write('.');
    }

    private void WriteCommandParameters()
    {
        if (!context.Command.Parameters.Any())
            return;

        if (!IsEmpty)
            WriteVerticalMargin();

        WriteHeader("Parameters");

        foreach (var parameter in context.Command.Parameters.OrderBy(p => p.Order))
        {
            if (parameter.IsRequired)
            {
                Write(ConsoleColor.Red, "* ");
            }
            else
            {
                WriteHorizontalMargin();
            }

            Write(ConsoleColor.DarkCyan, $"{parameter.Name}");

            WriteColumnMargin();

            // Description
            if (!string.IsNullOrWhiteSpace(parameter.Description))
            {
                Write(parameter.Description);
                Write(' ');
            }

            // Valid values
            var validValues = parameter.Property.TryGetValidValues() ?? [];
            if (validValues.Any())
            {
                Write(ConsoleColor.White, "Choices: ");

                var isFirst = true;
                foreach (var validValue in validValues)
                {
                    if (validValue is null)
                        continue;

                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        Write(", ");
                    }

                    Write('"');
                    Write(validValue.ToString());
                    Write('"');
                }

                Write('.');
                Write(' ');
            }

            // Default value
            if (!parameter.IsRequired)
            {
                WriteCommandInputDefaultValue(parameter);
            }

            WriteLine();
        }
    }

    private void WriteCommandOptions()
    {
        if (!IsEmpty)
            WriteVerticalMargin();

        WriteHeader("Options");

        foreach (var option in context.Command.Options.OrderByDescending(o => o.IsRequired))
        {
            if (option.IsRequired)
            {
                Write(ConsoleColor.Red, "* ");
            }
            else
            {
                WriteHorizontalMargin();
            }

            // Short name
            if (option.ShortName is not null)
            {
                Write(ConsoleColor.Yellow, $"-{option.ShortName}");
            }

            // Separator
            if (!string.IsNullOrWhiteSpace(option.Name) && option.ShortName is not null)
            {
                Write('|');
            }

            // Name
            if (!string.IsNullOrWhiteSpace(option.Name))
            {
                Write(ConsoleColor.Yellow, $"--{option.Name}");
            }

            WriteColumnMargin();

            // Description
            if (!string.IsNullOrWhiteSpace(option.Description))
            {
                Write(option.Description);
                Write(' ');
            }

            // Valid values
            var validValues = option.Property.TryGetValidValues() ?? [];
            if (validValues.Any())
            {
                Write(ConsoleColor.White, "Choices: ");

                var isFirst = true;
                foreach (var validValue in validValues)
                {
                    if (validValue is null)
                        continue;

                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        Write(", ");
                    }

                    Write('"');
                    Write(validValue.ToString());
                    Write('"');
                }

                Write('.');
                Write(' ');
            }

            // Environment variable
            if (!string.IsNullOrWhiteSpace(option.EnvironmentVariable))
            {
                Write(ConsoleColor.White, "Environment variable: ");
                Write(option.EnvironmentVariable);
                Write('.');
                Write(' ');
            }

            // Default value
            if (!option.IsRequired)
            {
                WriteCommandInputDefaultValue(option);
            }

            WriteLine();
        }
    }

    private void WriteCommandChildren()
    {
        var childCommands = context
            .Root.GetChildCommands(context.Command.Name)
            .OrderBy(a => a.Name, StringComparer.Ordinal)
            .ToArray();

        if (!childCommands.Any())
            return;

        if (!IsEmpty)
            WriteVerticalMargin();

        WriteHeader("Commands");

        foreach (var childCommand in childCommands)
        {
            // Name
            WriteHorizontalMargin();
            Write(
                ConsoleColor.Cyan,
                // Trim the name so it's relative to the current command
                childCommand.Name?.Substring(context.Command.Name?.Length ?? 0).Trim()
            );

            WriteColumnMargin();

            // Description
            if (!string.IsNullOrWhiteSpace(childCommand.Description))
            {
                Write(childCommand.Description);
                Write(' ');
            }

            // Grand-child commands
            var grandChildCommands = context
                .Root.GetChildCommands(childCommand.Name)
                .OrderBy(c => c.Name, StringComparer.Ordinal)
                .ToArray();

            if (grandChildCommands.Any())
            {
                Write(ConsoleColor.White, "Subcommands: ");

                var isFirst = true;
                foreach (var grandChildCommand in grandChildCommands)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        Write(", ");
                    }

                    Write(
                        ConsoleColor.Cyan,
                        // Trim the name so it's relative to the current command
                        grandChildCommand.Name?.Substring(context.Command.Name?.Length ?? 0).Trim()
                    );
                }

                Write('.');
            }

            WriteLine();
        }

        // Child command help tip
        WriteVerticalMargin();
        Write("You can run `");
        WriteCommandInvocation();
        Write(' ');
        Write(ConsoleColor.Cyan, "[command]");
        Write(' ');
        Write(ConsoleColor.White, "--help");
        Write("` to show help for a specific command.");

        WriteLine();
    }

    public void WriteHelpText()
    {
        // Application info
        Write(ConsoleColor.White, context.Metadata.Title);
        Write(' ');
        Write(ConsoleColor.Yellow, context.Metadata.Version);
        WriteLine();

        if (!string.IsNullOrWhiteSpace(context.Metadata.Description))
        {
            WriteHorizontalMargin();
            Write(context.Metadata.Description);
            WriteLine();
        }

        // Command info
        WriteCommandUsage();
        WriteCommandDescription();
        WriteCommandParameters();
        WriteCommandOptions();
        WriteCommandChildren();
    }
}
