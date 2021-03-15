using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CliFx.Infrastructure;
using CliFx.Schema;
using CliFx.Utils.Extensions;

namespace CliFx.Formatting
{
    // TODO: refactor
    internal partial class ConsoleFormatter
    {
        private static void WriteApplicationInfo(
            ConsoleFormatter formatter,
            ApplicationMetadata applicationMetadata)
        {
            // Title and version
            formatter.Write(ConsoleColor.Yellow, applicationMetadata.Title);
            formatter.Write(' ');
            formatter.Write(ConsoleColor.Yellow, applicationMetadata.Version);
            formatter.WriteLine();

            // Description
            if (!string.IsNullOrWhiteSpace(applicationMetadata.Description))
            {
                formatter.WriteHorizontalMargin();
                formatter.Write(applicationMetadata.Description);
                formatter.WriteLine();
            }
        }

        private static void WriteCommandDescription(
            ConsoleFormatter formatter,
            CommandSchema commandSchema)
        {
            if (string.IsNullOrWhiteSpace(commandSchema.Description))
                return;

            if (!formatter.IsEmpty)
                formatter.WriteVerticalMargin();

            formatter.WriteHeader("Description");

            formatter.WriteHorizontalMargin();
            formatter.Write(commandSchema.Description);
            formatter.WriteLine();
        }

        private static void WriteCommandUsageLineItem(
            ConsoleFormatter formatter,
            CommandSchema commandSchema,
            bool showChildCommandsPlaceholder)
        {
            // Command name
            if (!string.IsNullOrWhiteSpace(commandSchema.Name))
            {
                formatter.Write(ConsoleColor.Cyan, commandSchema.Name);
                formatter.Write(' ');
            }

            // Child command placeholder
            if (showChildCommandsPlaceholder)
            {
                formatter.Write(ConsoleColor.Cyan, "[command]");
                formatter.Write(' ');
            }

            // Parameters
            foreach (var parameter in commandSchema.Parameters)
            {
                formatter.Write(parameter.IsScalar
                    ? $"<{parameter.Name}>"
                    : $"<{parameter.Name}...>"
                );
                formatter.Write(' ');
            }

            // Required options
            foreach (var option in commandSchema.Options.Where(o => o.IsRequired))
            {
                formatter.Write(ConsoleColor.White, !string.IsNullOrWhiteSpace(option.Name)
                    ? $"--{option.Name}"
                    : $"-{option.ShortName}"
                );
                formatter.Write(' ');

                formatter.Write(option.IsScalar
                    ? "<value>"
                    : "<values...>"
                );
                formatter.Write(' ');
            }

            // Options placeholder
            formatter.Write(ConsoleColor.White, "[options]");

            formatter.WriteLine();
        }

        private static void WriteCommandUsage(
            ConsoleFormatter formatter,
            ApplicationMetadata applicationMetadata,
            CommandSchema commandSchema,
            IReadOnlyList<CommandSchema> childCommandSchemas)
        {
            if (!formatter.IsEmpty)
                formatter.WriteVerticalMargin();

            formatter.WriteHeader("Usage");

            // Exe name
            formatter.WriteHorizontalMargin();
            formatter.Write(applicationMetadata.ExecutableName);
            formatter.Write(' ');

            // Current command usage
            WriteCommandUsageLineItem(formatter, commandSchema, childCommandSchemas.Any());

            // Sub commands usage
            if (childCommandSchemas.Any())
            {
                formatter.WriteVerticalMargin();

                foreach (var childCommand in childCommandSchemas)
                {
                    formatter.WriteHorizontalMargin();
                    formatter.Write("... ");
                    WriteCommandUsageLineItem(formatter, childCommand, false);
                }
            }
        }

        private static void WriteCommandParameters(
            ConsoleFormatter formatter,
            CommandSchema commandSchema)
        {
            if (!commandSchema.Parameters.Any())
                return;

            if (!formatter.IsEmpty)
                formatter.WriteVerticalMargin();

            formatter.WriteHeader("Parameters");

            foreach (var parameter in commandSchema.Parameters.OrderBy(p => p.Order))
            {
                formatter.Write(ConsoleColor.Red, "* ");
                formatter.Write(ConsoleColor.White, $"{parameter.Name}");

                formatter.WriteColumnMargin();

                // Description
                if (!string.IsNullOrWhiteSpace(parameter.Description))
                {
                    formatter.Write(parameter.Description);
                    formatter.Write(' ');
                }

                // Valid values
                var validValues = parameter.GetValidValues();
                if (validValues.Any())
                {
                    formatter.Write($"Valid values: {FormatValidValues(validValues)}.");
                }

                formatter.WriteLine();
            }
        }

        private static void WriteCommandOptions(
            ConsoleFormatter formatter,
            CommandSchema commandSchema,
            IReadOnlyDictionary<MemberSchema, object?> defaultValues)
        {
            if (!formatter.IsEmpty)
                formatter.WriteVerticalMargin();

            formatter.WriteHeader("Options");

            foreach (var option in commandSchema.Options.OrderByDescending(o => o.IsRequired))
            {
                if (option.IsRequired)
                {
                    formatter.Write(ConsoleColor.Red, "* ");
                }
                else
                {
                    formatter.WriteHorizontalMargin();
                }

                // Short name
                if (option.ShortName is not null)
                {
                    formatter.Write(ConsoleColor.White, $"-{option.ShortName}");
                }

                // Separator
                if (!string.IsNullOrWhiteSpace(option.Name) && option.ShortName is not null)
                {
                    formatter.Write('|');
                }

                // Name
                if (!string.IsNullOrWhiteSpace(option.Name))
                {
                    formatter.Write(ConsoleColor.White, $"--{option.Name}");
                }

                formatter.WriteColumnMargin();

                // Description
                if (!string.IsNullOrWhiteSpace(option.Description))
                {
                    formatter.Write(option.Description);
                    formatter.Write(' ');
                }

                // Valid values
                var validValues = option.GetValidValues();
                if (validValues.Any())
                {
                    formatter.Write($"Valid values: {FormatValidValues(validValues)}.");
                    formatter.Write(' ');
                }

                // Environment variable
                if (!string.IsNullOrWhiteSpace(option.EnvironmentVariable))
                {
                    formatter.Write($"Environment variable: \"{option.EnvironmentVariable}\".");
                    formatter.Write(' ');
                }

                // Default value
                if (!option.IsRequired)
                {
                    var defaultValue = defaultValues.GetValueOrDefault(option);
                    var defaultValueFormatted = TryFormatDefaultValue(defaultValue);
                    if (defaultValueFormatted is not null)
                    {
                        formatter.Write($"Default: {defaultValueFormatted}.");
                    }
                }

                formatter.WriteLine();
            }
        }

        private static void WriteCommandChildren(
            ConsoleFormatter formatter,
            ApplicationMetadata applicationMetadata,
            CommandSchema commandSchema,
            IReadOnlyList<CommandSchema> childCommandSchemas)
        {
            if (!childCommandSchemas.Any())
                return;

            if (!formatter.IsEmpty)
                formatter.WriteVerticalMargin();

            formatter.WriteHeader("Commands");

            foreach (var childCommand in childCommandSchemas)
            {
                var relativeCommandName = !string.IsNullOrWhiteSpace(commandSchema.Name)
                    ? childCommand.Name!.Substring(commandSchema.Name.Length).Trim()
                    : childCommand.Name!;

                // Name
                formatter.WriteHorizontalMargin();
                formatter.Write(ConsoleColor.Cyan, relativeCommandName);

                // Description
                if (!string.IsNullOrWhiteSpace(childCommand.Description))
                {
                    formatter.WriteColumnMargin();
                    formatter.Write(childCommand.Description);
                }

                formatter.WriteLine();
            }

            // Child command help tip
            formatter.WriteVerticalMargin();
            formatter.Write("You can run `");
            formatter.Write(applicationMetadata.ExecutableName);

            if (!string.IsNullOrWhiteSpace(commandSchema.Name))
            {
                formatter.Write(' ');
                formatter.Write(ConsoleColor.Cyan, commandSchema.Name);
            }

            formatter.Write(' ');
            formatter.Write(ConsoleColor.Cyan, "[command]");

            formatter.Write(' ');
            formatter.Write(ConsoleColor.White, "--help");

            formatter.Write("` to show help on a specific command.");

            formatter.WriteLine();
        }

        public static void WriteHelpText(IConsole console, HelpContext helpContext)
        {
            var formatter = new ConsoleFormatter(console, false);

            var commandName = helpContext.CommandSchema.Name;
            var childCommands = helpContext.ApplicationSchema.GetChildCommands(commandName);
            var descendantCommands = helpContext.ApplicationSchema.GetDescendantCommands(commandName);

            WriteApplicationInfo(formatter, helpContext.ApplicationMetadata);
            WriteCommandDescription(formatter, helpContext.CommandSchema);
            WriteCommandUsage(formatter, helpContext.ApplicationMetadata, helpContext.CommandSchema, descendantCommands);
            WriteCommandParameters(formatter, helpContext.CommandSchema);
            WriteCommandOptions(formatter, helpContext.CommandSchema, helpContext.CommandDefaultValues);
            WriteCommandChildren(formatter, helpContext.ApplicationMetadata, helpContext.CommandSchema, childCommands);
        }

        // TODO: move
        private static string FormatValidValues(IReadOnlyList<object?> values) =>
            values
                .Select(v => v?.ToString().Quote())
                .Where(v => v is not null)
                .JoinToString(", ");

        private static string? TryFormatDefaultValue(object? defaultValue)
        {
            if (defaultValue is null)
                return null;

            // Enumerable
            if (!(defaultValue is string) && defaultValue is IEnumerable defaultValues)
            {
                var elementType = defaultValues.GetType().TryGetEnumerableUnderlyingType() ?? typeof(object);

                // If the ToString() method is not overriden, the default value can't be formatted nicely
                if (!elementType.IsToStringOverriden())
                    return null;

                return defaultValues
                    .Cast<object?>()
                    .Where(o => o is not null)
                    .Select(o => o!.ToFormattableString(CultureInfo.InvariantCulture).Quote())
                    .JoinToString(" ");
            }
            // Non-enumerable
            else
            {
                // If the ToString() method is not overriden, the default value can't be formatted nicely
                if (!defaultValue.GetType().IsToStringOverriden())
                    return null;

                return defaultValue.ToFormattableString(CultureInfo.InvariantCulture).Quote();
            }
        }
    }
}