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
    internal class HelpConsoleFormatter : ConsoleFormatter
    {
        private readonly HelpContext _context;

        public HelpConsoleFormatter(ConsoleWriter consoleWriter, HelpContext context)
            : base(consoleWriter)
        {
            _context = context;
        }

        public void WriteHeader(string text)
        {
            Write(ConsoleColor.Magenta, text);
            WriteLine();
        }

        private void WriteApplicationInfo()
        {
            // Title and version
            Write(ConsoleColor.Yellow, _context.ApplicationMetadata.Title);
            Write(' ');
            Write(ConsoleColor.Yellow, _context.ApplicationMetadata.Version);
            WriteLine();

            // Description
            if (string.IsNullOrWhiteSpace(_context.ApplicationMetadata.Description))
                return;

            WriteHorizontalMargin();
            Write(_context.ApplicationMetadata.Description);
            WriteLine();
        }

        private void WriteCommandDescription()
        {
            if (string.IsNullOrWhiteSpace(_context.CommandSchema.Description))
                return;

            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Description");

            WriteHorizontalMargin();
            Write(_context.CommandSchema.Description);
            WriteLine();
        }

        // TODO: refactor
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

        private void WriteCommandUsage()
        {
            var childCommandSchemas = _context.ApplicationSchema.GetChildCommands(_context.CommandSchema.Name);

            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Usage");

            // Exe name
            WriteHorizontalMargin();
            Write(_context.ApplicationMetadata.ExecutableName);
            Write(' ');

            // Current command usage
            WriteCommandUsageLineItem(_context.CommandSchema, childCommandSchemas.Any());

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

        private void WriteCommandParameters()
        {
            if (!_context.CommandSchema.Parameters.Any())
                return;

            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Parameters");

            foreach (var parameter in _context.CommandSchema.Parameters.OrderBy(p => p.Order))
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

        private void WriteCommandOptions()
        {
            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Options");

            foreach (var option in _context.CommandSchema.Options.OrderByDescending(o => o.IsRequired))
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
                if (!string.IsNullOrWhiteSpace(option.EnvironmentVariable))
                {
                    Write($"Environment variable: \"{option.EnvironmentVariable}\".");
                    Write(' ');
                }

                // Default value
                if (!option.IsRequired)
                {
                    var defaultValue = _context.CommandDefaultValues.GetValueOrDefault(option);
                    var defaultValueFormatted = TryFormatDefaultValue(defaultValue);
                    if (defaultValueFormatted is not null)
                    {
                        Write($"Default: {defaultValueFormatted}.");
                    }
                }

                WriteLine();
            }
        }

        private void WriteCommandChildren()
        {
            var childCommandSchemas = _context.ApplicationSchema.GetChildCommands(_context.CommandSchema.Name);

            if (!childCommandSchemas.Any())
                return;

            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Commands");

            foreach (var childCommand in childCommandSchemas)
            {
                var relativeCommandName = !string.IsNullOrWhiteSpace(_context.CommandSchema.Name)
                    ? childCommand.Name!.Substring(_context.CommandSchema.Name.Length).Trim()
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
            Write(_context.ApplicationMetadata.ExecutableName);

            if (!string.IsNullOrWhiteSpace(_context.CommandSchema.Name))
            {
                Write(' ');
                Write(ConsoleColor.Cyan, _context.CommandSchema.Name);
            }

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
            WriteCommandDescription();
            WriteCommandUsage();
            WriteCommandParameters();
            WriteCommandOptions();
            WriteCommandChildren();
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

    internal static class HelpConsoleFormatterExtensions
    {
        public static void WriteHelpText(this IConsole console, HelpContext context) =>
            new HelpConsoleFormatter(console.Output, context).WriteHelpText();
    }
}