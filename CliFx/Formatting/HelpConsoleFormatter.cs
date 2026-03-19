using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CliFx.Binding;
using CliFx.Infrastructure;
using CliFx.Utils.Extensions;

namespace CliFx.Formatting;

internal class HelpConsoleFormatter(ConsoleWriter consoleWriter, HelpContext context)
    : ConsoleFormatter(consoleWriter)
{
    private void WriteHeader(string text)
    {
        Write(ConsoleColor.White, text.ToUpperInvariant());
        WriteLine();
    }

    private void WriteCommandInvocation()
    {
        Write(context.ApplicationMetadata.ExecutableName);

        // Command name
        if (!string.IsNullOrWhiteSpace(context.CommandDescriptor.Name))
        {
            Write(' ');
            Write(ConsoleColor.Cyan, context.CommandDescriptor.Name);
        }
    }

    private void WriteApplicationInfo()
    {
        if (!IsEmpty)
            WriteVerticalMargin();

        // Title and version
        Write(ConsoleColor.White, context.ApplicationMetadata.Title);
        Write(' ');
        Write(ConsoleColor.Yellow, context.ApplicationMetadata.Version);
        WriteLine();

        // Description
        if (!string.IsNullOrWhiteSpace(context.ApplicationMetadata.Description))
        {
            WriteHorizontalMargin();
            Write(context.ApplicationMetadata.Description);
            WriteLine();
        }
    }

    private void WriteCommandUsage()
    {
        if (!IsEmpty)
            WriteVerticalMargin();

        WriteHeader("Usage");

        // Current command usage
        {
            WriteHorizontalMargin();

            WriteCommandInvocation();
            Write(' ');

            // Parameters
            foreach (var parameter in context.CommandDescriptor.Parameters.OrderBy(p => p.Order))
            {
                Write(
                    ConsoleColor.DarkCyan,
                    !parameter.Converter.SupportsSequence
                        ? $"<{parameter.Name}>"
                        : $"<{parameter.Name}...>"
                );
                Write(' ');
            }

            // Required options
            foreach (var option in context.CommandDescriptor.Options.Where(o => o.IsRequired))
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
                    !option.Converter.SupportsSequence ? "<value>" : "<values...>"
                );
                Write(' ');
            }

            // Placeholder for non-required options
            if (context.CommandDescriptor.Options.Any(o => !o.IsRequired))
            {
                Write(ConsoleColor.Yellow, "[options]");
            }

            WriteLine();
        }

        // Child command usage
        var childCommandDescriptors = context.ApplicationDescriptor.GetChildCommands(
            context.CommandDescriptor.Name
        );

        if (childCommandDescriptors.Any())
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
        if (string.IsNullOrWhiteSpace(context.CommandDescriptor.Description))
            return;

        if (!IsEmpty)
            WriteVerticalMargin();

        WriteHeader("Description");

        WriteHorizontalMargin();

        Write(context.CommandDescriptor.Description);
        WriteLine();
    }

    [RequiresUnreferencedCode("Displays default values using runtime type reflection.")]
    private void WriteCommandParameters()
    {
        if (!context.CommandDescriptor.Parameters.Any())
            return;

        if (!IsEmpty)
            WriteVerticalMargin();

        WriteHeader("Parameters");

        foreach (var parameterSchema in context.CommandDescriptor.Parameters.OrderBy(p => p.Order))
        {
            if (parameterSchema.IsRequired)
            {
                Write(ConsoleColor.Red, "* ");
            }
            else
            {
                WriteHorizontalMargin();
            }

            Write(ConsoleColor.DarkCyan, $"{parameterSchema.Name}");

            WriteColumnMargin();

            // Description
            if (!string.IsNullOrWhiteSpace(parameterSchema.Description))
            {
                Write(parameterSchema.Description);
                Write(' ');
            }

            // Valid values
            var validValues = parameterSchema.Property.TryGetValidValues() ?? [];
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
            if (!parameterSchema.IsRequired)
            {
                WriteDefaultValue(parameterSchema);
            }

            WriteLine();
        }
    }

    [RequiresUnreferencedCode("Displays default values using runtime type reflection.")]
    private void WriteCommandOptions()
    {
        if (!IsEmpty)
            WriteVerticalMargin();

        WriteHeader("Options");

        foreach (
            var optionSchema in context.CommandDescriptor.Options.OrderByDescending(o =>
                o.IsRequired
            )
        )
        {
            if (optionSchema.IsRequired)
            {
                Write(ConsoleColor.Red, "* ");
            }
            else
            {
                WriteHorizontalMargin();
            }

            // Short name
            if (optionSchema.ShortName is not null)
            {
                Write(ConsoleColor.Yellow, $"-{optionSchema.ShortName}");
            }

            // Separator
            if (!string.IsNullOrWhiteSpace(optionSchema.Name) && optionSchema.ShortName is not null)
            {
                Write('|');
            }

            // Name
            if (!string.IsNullOrWhiteSpace(optionSchema.Name))
            {
                Write(ConsoleColor.Yellow, $"--{optionSchema.Name}");
            }

            WriteColumnMargin();

            // Description
            if (!string.IsNullOrWhiteSpace(optionSchema.Description))
            {
                Write(optionSchema.Description);
                Write(' ');
            }

            // Valid values
            var validValues = optionSchema.Property.TryGetValidValues() ?? [];
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
            if (!string.IsNullOrWhiteSpace(optionSchema.EnvironmentVariable))
            {
                Write(ConsoleColor.White, "Environment variable: ");
                Write(optionSchema.EnvironmentVariable);
                Write('.');
                Write(' ');
            }

            // Default value
            if (!optionSchema.IsRequired)
            {
                WriteDefaultValue(optionSchema);
            }

            WriteLine();
        }
    }

    private void WriteDefaultValue(CommandInputDescriptor schema)
    {
        var defaultValue = context.CommandDefaultValues.GetValueOrDefault(schema);
        if (defaultValue is null)
            return;

        // Normalize to an array to process both single and sequence default values uniformly
        var defaultValues =
            defaultValue is not string && defaultValue is IEnumerable defaultValueAsEnumerable
                ? defaultValueAsEnumerable.Cast<object>().ToArray()
                : [defaultValue];

        // Only strings, chars, bools, and types that implement IFormattable have
        // meaningful ToString() representations.
        if (!defaultValues.All(v => v is string or char or bool or IFormattable))
            return;

        var isFirst = true;

        foreach (var element in defaultValues)
        {
            if (element is not IFormattable and not IConvertible)
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
            Write(element.ToString(CultureInfo.InvariantCulture));
            Write('"');
        }

        if (!isFirst)
            Write('.');
    }

    private void WriteCommandChildren()
    {
        var childCommandDescriptors = context
            .ApplicationDescriptor.GetChildCommands(context.CommandDescriptor.Name)
            .OrderBy(a => a.Name, StringComparer.Ordinal)
            .ToArray();

        if (!childCommandDescriptors.Any())
            return;

        if (!IsEmpty)
            WriteVerticalMargin();

        WriteHeader("Commands");

        foreach (var childCommandDescriptor in childCommandDescriptors)
        {
            // Name
            WriteHorizontalMargin();
            Write(
                ConsoleColor.Cyan,
                // Relative to current command
                childCommandDescriptor
                    .Name?.Substring(context.CommandDescriptor.Name?.Length ?? 0)
                    .Trim()
            );

            WriteColumnMargin();

            // Description
            if (!string.IsNullOrWhiteSpace(childCommandDescriptor.Description))
            {
                Write(childCommandDescriptor.Description);
                Write(' ');
            }

            // Child commands of child command
            var grandChildCommandDescriptors = context
                .ApplicationDescriptor.GetChildCommands(childCommandDescriptor.Name)
                .OrderBy(c => c.Name, StringComparer.Ordinal)
                .ToArray();

            if (grandChildCommandDescriptors.Any())
            {
                Write(ConsoleColor.White, "Subcommands: ");

                var isFirst = true;
                foreach (var grandChildCommandDescriptor in grandChildCommandDescriptors)
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
                        // Relative to current command (not the parent)
                        grandChildCommandDescriptor
                            .Name?.Substring(context.CommandDescriptor.Name?.Length ?? 0)
                            .Trim()
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
        Write("` to show help on a specific command.");

        WriteLine();
    }

    [RequiresUnreferencedCode("Displays default values using runtime type reflection.")]
    public void WriteHelpText()
    {
        WriteApplicationInfo();
        WriteCommandUsage();
        WriteCommandDescription();
        WriteCommandParameters();
        WriteCommandOptions();
        WriteCommandChildren();
    }
}
