using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Internal;

namespace CliFx.Domain
{
    internal class HelpTextWriter
    {
        private readonly ApplicationMetadata _metadata;
        private readonly IConsole _console;
        private readonly ITypeActivator _typeActivator;

        private int _column;
        private int _row;

        private bool IsEmpty => _column == 0 && _row == 0;

        public HelpTextWriter(ApplicationMetadata metadata, IConsole console, ITypeActivator typeActivator)
        {
            _metadata = metadata;
            _console = console;
            _typeActivator = typeActivator;
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
            if (IsEmpty)
                return;

            for (var i = 0; i < size; i++)
                WriteLine();
        }

        private void WriteHorizontalMargin(int size = 2)
        {
            for (var i = 0; i < size; i++)
                Write(' ');
        }

        private void WriteHorizontalColumnMargin(int columnSize = 20, int offsetSize = 2)
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

        private void WriteCommandDescription(CommandSchema commandSchema)
        {
            if (string.IsNullOrWhiteSpace(commandSchema.Description))
                return;

            WriteVerticalMargin();
            WriteHeader("Description");

            WriteHorizontalMargin();
            Write(commandSchema.Description);
            WriteLine();
        }

        private void WriteCommandUsage(
            CommandSchema commandSchema,
            IReadOnlyList<CommandSchema> childCommandSchemas)
        {
            WriteVerticalMargin();
            WriteHeader("Usage");

            // Exe name
            WriteHorizontalMargin();
            Write(_metadata.ExecutableName);

            // Command name
            if (!string.IsNullOrWhiteSpace(commandSchema.Name))
            {
                Write(' ');
                Write(ConsoleColor.Cyan, commandSchema.Name);
            }

            // Child command placeholder
            if (childCommandSchemas.Any())
            {
                Write(' ');
                Write(ConsoleColor.Cyan, "[command]");
            }

            // Parameters
            foreach (var parameterSchema in commandSchema.Parameters)
            {
                Write(' ');
                Write(parameterSchema.IsScalar
                    ? $"<{parameterSchema.Name}>"
                    : $"<{parameterSchema.Name}...>"
                );
            }

            // Required options
            foreach (var optionSchema in commandSchema.Options.Where(o => o.IsRequired))
            {
                Write(' ');
                Write(ConsoleColor.White, !string.IsNullOrWhiteSpace(optionSchema.Name)
                    ? $"--{optionSchema.Name}"
                    : $"-{optionSchema.ShortName}"
                );

                Write(' ');
                Write(optionSchema.IsScalar
                    ? "<value>"
                    : "<values...>"
                );
            }

            // Options placeholder
            Write(' ');
            Write(ConsoleColor.White, "[options]");

            WriteLine();
        }

        private void WriteCommandParameters(CommandSchema commandSchema)
        {
            if (!commandSchema.Parameters.Any())
                return;

            WriteVerticalMargin();
            WriteHeader("Parameters");

            foreach (var parameterSchema in commandSchema.Parameters.OrderBy(p => p.Order))
            {
                Write(ConsoleColor.Red, "* ");
                Write(ConsoleColor.White, $"{parameterSchema.Name}");

                WriteHorizontalColumnMargin();

                // Description
                if (!string.IsNullOrWhiteSpace(parameterSchema.Description))
                {
                    Write(parameterSchema.Description);
                    Write(' ');
                }

                // Valid values
                var validValues = parameterSchema.GetValidValues();
                if (validValues.Any())
                {
                    Write($"Valid values: {validValues.JoinToString(", ")}.");
                }

                WriteLine();
            }
        }

        private void WriteCommandOptions(CommandSchema commandSchema, ICommand command)
        {
            WriteVerticalMargin();
            WriteHeader("Options");

            foreach (var optionSchema in commandSchema.Options.OrderByDescending(o => o.IsRequired))
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
                if (optionSchema.ShortName != null)
                {
                    Write(ConsoleColor.White, $"-{optionSchema.ShortName}");
                }

                // Delimiter
                if (!string.IsNullOrWhiteSpace(optionSchema.Name) && optionSchema.ShortName != null)
                {
                    Write('|');
                }

                // Name
                if (!string.IsNullOrWhiteSpace(optionSchema.Name))
                {
                    Write(ConsoleColor.White, $"--{optionSchema.Name}");
                }

                WriteHorizontalColumnMargin();

                // Description
                if (!string.IsNullOrWhiteSpace(optionSchema.Description))
                {
                    Write(optionSchema.Description);
                    Write(' ');
                }

                // Valid values
                var validValues = optionSchema.GetValidValues();
                if (validValues.Any())
                {
                    Write($"Valid values: {validValues.Select(v => v.Quote()).JoinToString(", ")}.");
                    Write(' ');
                }

                // Environment variable
                if (!string.IsNullOrWhiteSpace(optionSchema.EnvironmentVariableName))
                {
                    Write($"Environment variable: \"{optionSchema.EnvironmentVariableName}\".");
                    Write(' ');
                }

                // Default value
                if (!optionSchema.IsRequired)
                {
                    // TODO: move quoting logic here?
                    var defaultValue = optionSchema.TryGetDefaultValue(command);
                    if (defaultValue != null)
                    {
                        Write($"Default: {defaultValue}.");
                    }
                }

                WriteLine();
            }
        }

        private void WriteCommandChildren(
            CommandSchema commandSchema,
            IReadOnlyList<CommandSchema> childCommandSchemas)
        {
            if (!childCommandSchemas.Any())
                return;

            WriteVerticalMargin();
            WriteHeader("Commands");

            foreach (var childCommandSchema in childCommandSchemas)
            {
                var relativeCommandName = !string.IsNullOrWhiteSpace(commandSchema.Name)
                    ? childCommandSchema.Name!.Substring(commandSchema.Name.Length + 1)
                    : childCommandSchema.Name!;

                // Name
                WriteHorizontalMargin();
                Write(ConsoleColor.Cyan, relativeCommandName);

                // Description
                if (!string.IsNullOrWhiteSpace(childCommandSchema.Description))
                {
                    WriteHorizontalColumnMargin();
                    Write(childCommandSchema.Description);
                }

                WriteLine();
            }

            // Child command help tip
            WriteVerticalMargin();
            Write("You can run `");
            Write(_metadata.ExecutableName);

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

        public void Write(ApplicationSchema applicationSchema, CommandSchema? commandSchema)
        {
            var childCommandSchemas = applicationSchema.GetChildCommands(commandSchema?.Name);

            var command = commandSchema != null
                ? (ICommand) _typeActivator.CreateInstance(commandSchema.Type)
                : null;

            _console.ResetColor();

            if (commandSchema.IsDefault)
                WriteApplicationInfo();

            WriteCommandDescription(commandSchema);
            WriteCommandUsage(commandSchema, childCommandSchemas);
            WriteCommandParameters(commandSchema);
            WriteCommandOptions(commandSchema, command);
            WriteCommandChildren(commandSchema, childCommandSchemas);
        }
    }
}