using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CliFx.Infrastructure;
using CliFx.Schema;
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
        if (!string.IsNullOrWhiteSpace(context.CommandSchema.Name))
        {
            Write(' ');
            Write(ConsoleColor.Cyan, context.CommandSchema.Name);
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
            foreach (var parameter in context.CommandSchema.Parameters.OrderBy(p => p.Order))
            {
                Write(
                    ConsoleColor.DarkCyan,
                    parameter.IsSequence ? $"<{parameter.Name}...>" : $"<{parameter.Name}>"
                );
                Write(' ');
            }

            // Required options
            foreach (var option in context.CommandSchema.Options.Where(o => o.IsRequired))
            {
                Write(
                    ConsoleColor.Yellow,
                    !string.IsNullOrWhiteSpace(option.Name)
                        ? $"--{option.Name}"
                        : $"-{option.ShortName}"
                );
                Write(' ');

                Write(ConsoleColor.White, option.IsSequence ? "<values...>" : "<value>");
                Write(' ');
            }

            // Placeholder for non-required options
            if (context.CommandSchema.Options.Any(o => !o.IsRequired))
            {
                Write(ConsoleColor.Yellow, "[options]");
            }

            WriteLine();
        }

        // Child command usage
        var childCommandSchemas = context.ApplicationSchema.GetChildCommands(
            context.CommandSchema.Name
        );

        if (childCommandSchemas.Any())
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
        if (string.IsNullOrWhiteSpace(context.CommandSchema.Description))
            return;

        if (!IsEmpty)
            WriteVerticalMargin();

        WriteHeader("Description");

        WriteHorizontalMargin();

        Write(context.CommandSchema.Description);
        WriteLine();
    }

    private void WriteCommandParameters()
    {
        if (!context.CommandSchema.Parameters.Any())
            return;

        if (!IsEmpty)
            WriteVerticalMargin();

        WriteHeader("Parameters");

        foreach (var parameterSchema in context.CommandSchema.Parameters.OrderBy(p => p.Order))
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
            var validValues = parameterSchema.Property.TryGetValidValues();
            if (validValues?.Any() == true)
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

    private void WriteCommandOptions()
    {
        if (!IsEmpty)
            WriteVerticalMargin();

        WriteHeader("Options");

        foreach (
            var optionSchema in context.CommandSchema.Options.OrderByDescending(o => o.IsRequired)
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
            var validValues = optionSchema.Property.TryGetValidValues();
            if (validValues?.Any() == true)
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

    private void WriteDefaultValue(InputSchema schema)
    {
        var defaultValue = context.CommandDefaultValues.GetValueOrDefault(schema);
        if (defaultValue is null) return;

        // Non-Scalar
        if (defaultValue is not string && defaultValue is IEnumerable defaultValues)
        {
            var elementType =
                schema.Property.Type.TryGetEnumerableUnderlyingType() ?? typeof(object);

            if (elementType.IsToStringOverriden())
            {
                Write(ConsoleColor.White, "Default: ");

                var isFirst = true;

                foreach (var element in defaultValues)
                {
                    if (isFirst)
                    {
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

                Write('.');
            }
        }
        else
        {
            if (schema.Property.Type.IsToStringOverriden())
            {
                Write(ConsoleColor.White, "Default: ");

                Write('"');
                Write(defaultValue.ToString(CultureInfo.InvariantCulture));
                Write('"');
                Write('.');
            }
        }
    }

    private void WriteCommandChildren()
    {
        var childCommandSchemas = context
            .ApplicationSchema.GetChildCommands(context.CommandSchema.Name)
            .OrderBy(a => a.Name, StringComparer.Ordinal)
            .ToArray();

        if (!childCommandSchemas.Any())
            return;

        if (!IsEmpty)
            WriteVerticalMargin();

        WriteHeader("Commands");

        foreach (var childCommandSchema in childCommandSchemas)
        {
            // Name
            WriteHorizontalMargin();
            Write(
                ConsoleColor.Cyan,
                // Relative to current command
                childCommandSchema.Name?.Substring(context.CommandSchema.Name?.Length ?? 0).Trim()
            );

            WriteColumnMargin();

            // Description
            if (!string.IsNullOrWhiteSpace(childCommandSchema.Description))
            {
                Write(childCommandSchema.Description);
                Write(' ');
            }

            // Child commands of child command
            var grandChildCommandSchemas = context
                .ApplicationSchema.GetChildCommands(childCommandSchema.Name)
                .OrderBy(c => c.Name, StringComparer.Ordinal)
                .ToArray();

            if (grandChildCommandSchemas.Any())
            {
                Write(ConsoleColor.White, "Subcommands: ");

                var isFirst = true;
                foreach (var grandChildCommandSchema in grandChildCommandSchemas)
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
                        grandChildCommandSchema
                            .Name?.Substring(context.CommandSchema.Name?.Length ?? 0)
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

internal static class HelpConsoleFormatterExtensions
{
    public static void WriteHelpText(this ConsoleWriter consoleWriter, HelpContext context) =>
        new HelpConsoleFormatter(consoleWriter, context).WriteHelpText();

    public static void WriteHelpText(this IConsole console, HelpContext context) =>
        console.Output.WriteHelpText(context);
}
