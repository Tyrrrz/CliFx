using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CliFx.Schema;
using CliFx.Utils.Extensions;

namespace CliFx
{
    internal partial class ConsoleFormatter
    {
        private void WriteApplicationInfo(ApplicationMetadata applicationMetadata)
        {
            // Title and version
            Write(ConsoleColor.Yellow, applicationMetadata.Title);
            Write(' ');
            Write(ConsoleColor.Yellow, applicationMetadata.Version);
            WriteLine();

            // Description
            if (!string.IsNullOrWhiteSpace(applicationMetadata.Description))
            {
                WriteHorizontalMargin();
                Write(applicationMetadata.Description);
                WriteLine();
            }
        }

        private void WriteCommandDescription(CommandSchema commandSchema)
        {
            if (string.IsNullOrWhiteSpace(commandSchema.Description))
                return;

            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Description");

            WriteHorizontalMargin();
            Write(commandSchema.Description);
            WriteLine();
        }

        private void WriteCommandUsageLineItem(CommandSchema commandSchema, bool showChildCommandsPlaceholder)
        {
            // Command name
            if (!string.IsNullOrWhiteSpace(commandSchema.Name))
            {
                Write(ConsoleColor.Cyan, commandSchema.Name);
                Write(' ');
            }

            // Child command placeholder
            if (showChildCommandsPlaceholder)
            {
                Write(ConsoleColor.Cyan, "[command]");
                Write(' ');
            }

            // Parameters
            foreach (var parameter in commandSchema.Parameters)
            {
                Write(parameter.IsScalar
                    ? $"<{parameter.Name}>"
                    : $"<{parameter.Name}...>"
                );
                Write(' ');
            }

            // Required options
            foreach (var option in commandSchema.Options.Where(o => o.IsRequired))
            {
                Write(ConsoleColor.White, !string.IsNullOrWhiteSpace(option.Name)
                    ? $"--{option.Name}"
                    : $"-{option.ShortName}"
                );
                Write(' ');

                Write(option.IsScalar
                    ? "<value>"
                    : "<values...>"
                );
                Write(' ');
            }

            // Options placeholder
            Write(ConsoleColor.White, "[options]");

            WriteLine();
        }

        private void WriteCommandUsage(
            ApplicationMetadata applicationMetadata,
            CommandSchema commandSchema,
            IReadOnlyList<CommandSchema> childCommandSchemas)
        {
            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Usage");

            // Exe name
            WriteHorizontalMargin();
            Write(applicationMetadata.ExecutableName);
            Write(' ');

            // Current command usage
            WriteCommandUsageLineItem(commandSchema, childCommandSchemas.Any());

            // Sub commands usage
            if (childCommandSchemas.Any())
            {
                WriteVerticalMargin();

                foreach (var childCommand in childCommandSchemas)
                {
                    WriteHorizontalMargin();
                    Write("... ");
                    WriteCommandUsageLineItem(childCommand, false);
                }
            }
        }

        private void WriteCommandParameters(CommandSchema commandSchema)
        {
            if (!commandSchema.Parameters.Any())
                return;

            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Parameters");

            foreach (var parameter in commandSchema.Parameters.OrderBy(p => p.Order))
            {
                Write(ConsoleColor.Red, "* ");
                Write(ConsoleColor.White, $"{parameter.Name}");

                WriteColumnMargin();

                // Description
                if (!string.IsNullOrWhiteSpace(parameter.Description))
                {
                    Write(parameter.Description);
                    Write(' ');
                }

                // Valid values
                var validValues = parameter.GetValidValues();
                if (validValues.Any())
                {
                    Write($"Valid values: {FormatValidValues(validValues)}.");
                }

                WriteLine();
            }
        }

        private void WriteCommandOptions(
            CommandSchema commandSchema,
            IReadOnlyDictionary<MemberSchema, object?> defaultValues)
        {
            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Options");

            foreach (var option in commandSchema.Options.OrderByDescending(o => o.IsRequired))
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
                    Write(ConsoleColor.White, $"-{option.ShortName}");
                }

                // Separator
                if (!string.IsNullOrWhiteSpace(option.Name) && option.ShortName is not null)
                {
                    Write('|');
                }

                // Name
                if (!string.IsNullOrWhiteSpace(option.Name))
                {
                    Write(ConsoleColor.White, $"--{option.Name}");
                }

                WriteColumnMargin();

                // Description
                if (!string.IsNullOrWhiteSpace(option.Description))
                {
                    Write(option.Description);
                    Write(' ');
                }

                // Valid values
                var validValues = option.GetValidValues();
                if (validValues.Any())
                {
                    Write($"Valid values: {FormatValidValues(validValues)}.");
                    Write(' ');
                }

                // Environment variable
                if (!string.IsNullOrWhiteSpace(option.EnvironmentVariableName))
                {
                    Write($"Environment variable: \"{option.EnvironmentVariableName}\".");
                    Write(' ');
                }

                // Default value
                if (!option.IsRequired)
                {
                    var defaultValue = defaultValues.GetValueOrDefault(option);
                    var defaultValueFormatted = TryFormatDefaultValue(defaultValue);
                    if (defaultValueFormatted is not null)
                    {
                        Write($"Default: {defaultValueFormatted}.");
                    }
                }

                WriteLine();
            }
        }

        private void WriteCommandChildren(
            ApplicationMetadata applicationMetadata,
            CommandSchema commandSchema,
            IReadOnlyList<CommandSchema> childCommandSchemas)
        {
            if (!childCommandSchemas.Any())
                return;

            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Commands");

            foreach (var childCommand in childCommandSchemas)
            {
                var relativeCommandName = !string.IsNullOrWhiteSpace(commandSchema.Name)
                    ? childCommand.Name!.Substring(commandSchema.Name.Length).Trim()
                    : childCommand.Name!;

                // Name
                WriteHorizontalMargin();
                Write(ConsoleColor.Cyan, relativeCommandName);

                // Description
                if (!string.IsNullOrWhiteSpace(childCommand.Description))
                {
                    WriteColumnMargin();
                    Write(childCommand.Description);
                }

                WriteLine();
            }

            // Child command help tip
            WriteVerticalMargin();
            Write("You can run `");
            Write(applicationMetadata.ExecutableName);

            if (!string.IsNullOrWhiteSpace(commandSchema.Name))
            {
                Write(' ');
                Write(ConsoleColor.Cyan, commandSchema.Name);
            }

            Write(' ');
            Write(ConsoleColor.Cyan, "[command]");

            Write(' ');
            Write(ConsoleColor.White, "--help");

            Write("` to show help on a specific command.");

            WriteLine();
        }

        public void WriteHelpText(
            ApplicationMetadata applicationMetadata,
            ApplicationSchema applicationSchema,
            CommandSchema commandSchema,
            IReadOnlyDictionary<MemberSchema, object?> defaultValues)
        {
            var commandName = commandSchema.Name;
            var childCommands = applicationSchema.GetChildCommands(commandName);
            var descendantCommands = applicationSchema.GetDescendantCommands(commandName);

            if (commandSchema.IsDefault)
                WriteApplicationInfo(applicationMetadata);

            WriteCommandDescription(commandSchema);
            WriteCommandUsage(applicationMetadata, commandSchema, descendantCommands);
            WriteCommandParameters(commandSchema);
            WriteCommandOptions(commandSchema, defaultValues);
            WriteCommandChildren(applicationMetadata, commandSchema, childCommands);
        }

        // TODO: move
        private static string FormatValidValues(IReadOnlyList<string> values) =>
            values.Select(v => v.Quote()).JoinToString(", ");

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