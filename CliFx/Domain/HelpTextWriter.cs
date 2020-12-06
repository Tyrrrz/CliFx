using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CliFx.Internal.Extensions;

namespace CliFx.Domain
{
    internal partial class HelpTextWriter
    {
        private readonly ApplicationMetadata _metadata;
        private readonly IConsole _console;

        private int _column;
        private int _row;

        private bool IsEmpty => _column == 0 && _row == 0;

        public HelpTextWriter(ApplicationMetadata metadata, IConsole console)
        {
            _metadata = metadata;
            _console = console;
        }

        private void Write(char value)
        {
            _console.Output.Write(value);
            _column++;
        }

        private void Write(string value)
        {
            _console.Output.Write(value);
            _column += value.Length;
        }

        private void Write(ConsoleColor foregroundColor, string value)
        {
            _console.WithForegroundColor(foregroundColor, () => Write(value));
        }

        private void WriteLine()
        {
            _console.Output.WriteLine();
            _column = 0;
            _row++;
        }

        private void WriteVerticalMargin(int size = 1)
        {
            for (var i = 0; i < size; i++)
                WriteLine();
        }

        private void WriteHorizontalMargin(int size = 2)
        {
            for (var i = 0; i < size; i++)
                Write(' ');
        }

        private void WriteColumnMargin(int columnSize = 20, int offsetSize = 2)
        {
            if (_column + offsetSize < columnSize)
                WriteHorizontalMargin(columnSize - _column);
            else
                WriteHorizontalMargin(offsetSize);
        }

        private void WriteHeader(string text)
        {
            Write(ConsoleColor.Magenta, text);
            WriteLine();
        }

        private void WriteApplicationInfo()
        {
            // Title and version
            Write(ConsoleColor.Yellow, _metadata.Title);
            Write(' ');
            Write(ConsoleColor.Yellow, _metadata.VersionText);
            WriteLine();

            // Description
            if (!string.IsNullOrWhiteSpace(_metadata.Description))
            {
                WriteHorizontalMargin();
                Write(_metadata.Description);
                WriteLine();
            }
        }

        private void WriteCommandDescription(CommandSchema command)
        {
            if (string.IsNullOrWhiteSpace(command.Description))
                return;

            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Description");

            WriteHorizontalMargin();
            Write(command.Description);
            WriteLine();
        }

        private void WriteCommandUsageLineItem(CommandSchema command, bool showChildCommandsPlaceholder)
        {
            // Command name
            if (!string.IsNullOrWhiteSpace(command.Name))
            {
                Write(ConsoleColor.Cyan, command.Name);
                Write(' ');
            }

            // Child command placeholder
            if (showChildCommandsPlaceholder)
            {
                Write(ConsoleColor.Cyan, "[command]");
                Write(' ');
            }

            // Parameters
            foreach (var parameter in command.Parameters)
            {
                Write(parameter.IsScalar
                    ? $"<{parameter.Name}>"
                    : $"<{parameter.Name}...>"
                );
                Write(' ');
            }

            // Required options
            foreach (var option in command.Options.Where(o => o.IsRequired))
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
            CommandSchema command,
            IReadOnlyList<CommandSchema> childCommands)
        {
            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Usage");

            // Exe name
            WriteHorizontalMargin();
            Write(_metadata.ExecutableName);
            Write(' ');

            // Current command usage
            WriteCommandUsageLineItem(command, childCommands.Any());

            // Sub commands usage
            if (childCommands.Any())
            {
                WriteVerticalMargin();

                foreach (var childCommand in childCommands)
                {
                    WriteHorizontalMargin();
                    Write("... ");
                    WriteCommandUsageLineItem(childCommand, false);
                }
            }
        }

        private void WriteCommandParameters(CommandSchema command)
        {
            if (!command.Parameters.Any())
                return;

            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Parameters");

            foreach (var parameter in command.Parameters.OrderBy(p => p.Order))
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
            CommandSchema command,
            IReadOnlyDictionary<CommandArgumentSchema, object?> argumentDefaultValues)
        {
            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Options");

            foreach (var option in command.Options.OrderByDescending(o => o.IsRequired))
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
                if (option.ShortName != null)
                {
                    Write(ConsoleColor.White, $"-{option.ShortName}");
                }

                // Separator
                if (!string.IsNullOrWhiteSpace(option.Name) && option.ShortName != null)
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
                    var defaultValue = argumentDefaultValues.GetValueOrDefault(option);
                    var defaultValueFormatted = TryFormatDefaultValue(defaultValue);
                    if (defaultValueFormatted != null)
                    {
                        Write($"Default: {defaultValueFormatted}.");
                    }
                }

                WriteLine();
            }
        }

        private void WriteCommandChildren(
            CommandSchema command,
            IReadOnlyList<CommandSchema> childCommands)
        {
            if (!childCommands.Any())
                return;

            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Commands");

            foreach (var childCommand in childCommands)
            {
                var relativeCommandName = !string.IsNullOrWhiteSpace(command.Name)
                    ? childCommand.Name!.Substring(command.Name.Length).Trim()
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
            Write(_metadata.ExecutableName);

            if (!string.IsNullOrWhiteSpace(command.Name))
            {
                Write(' ');
                Write(ConsoleColor.Cyan, command.Name);
            }

            Write(' ');
            Write(ConsoleColor.Cyan, "[command]");

            Write(' ');
            Write(ConsoleColor.White, "--help");

            Write("` to show help on a specific command.");

            WriteLine();
        }

        public void Write(
            RootSchema root,
            CommandSchema command,
            IReadOnlyDictionary<CommandArgumentSchema, object?> defaultValues)
        {
            var commandName = command.Name;
            var childCommands = root.GetChildCommands(commandName);
            var descendantCommands = root.GetDescendantCommands(commandName);

            _console.ResetColor();

            if (command.IsDefault)
                WriteApplicationInfo();

            WriteCommandDescription(command);
            WriteCommandUsage(command, descendantCommands);
            WriteCommandParameters(command);
            WriteCommandOptions(command, defaultValues);
            WriteCommandChildren(command, childCommands);
        }
    }

    internal partial class HelpTextWriter
    {
        private static string FormatValidValues(IReadOnlyList<string> values) =>
            values.Select(v => v.Quote()).JoinToString(", ");

        private static string? TryFormatDefaultValue(object? defaultValue)
        {
            if (defaultValue == null)
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
                    .Where(o => o != null)
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