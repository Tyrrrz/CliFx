namespace CliFx.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using CliFx.Internal.Extensions;
    using CliFx.Schemas;

    internal partial class HelpTextWriter
    {
        private const ConsoleColor TitleColor = ConsoleColor.Yellow;
        private const ConsoleColor VersionColor = ConsoleColor.Yellow;
        private const ConsoleColor HeaderColor = ConsoleColor.Magenta;
        private const ConsoleColor DirectiveNameColor = ConsoleColor.Green;
        private const ConsoleColor CommandNameColor = ConsoleColor.Cyan;
        private const ConsoleColor ParametersColor = ConsoleColor.White;
        private const ConsoleColor OptionsPlaceholderColor = ConsoleColor.White;
        private const ConsoleColor RequiredColor = ConsoleColor.Red;
        private const ConsoleColor InteractiveOnlyColor = ConsoleColor.Magenta;
        private const ConsoleColor RequiredParameterNameColor = ConsoleColor.White;
        private const ConsoleColor OptionNameColor = ConsoleColor.White;

        private readonly ICliContext _cliContext;
        private readonly IConsole _console;

        private int _column;
        private int _row;

        private bool IsEmpty => _column == 0 && _row == 0;

        public HelpTextWriter(ICliContext cliContext)
        {
            _cliContext = cliContext;
            _console = cliContext.Console;
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
            for (int i = 0; i < size; i++)
                WriteLine();
        }

        private void WriteHorizontalMargin(int size = 2)
        {
            Write(new string(' ', size));
        }

        private void WriteColumnMargin(int columnSize = 24, int offsetSize = 2)
        {
            if (_column + offsetSize < columnSize)
                WriteHorizontalMargin(columnSize - _column);
            else
                WriteHorizontalMargin(offsetSize);
        }

        private void WriteHeader(string text)
        {
            Write(HeaderColor, text);
            WriteLine();
        }

        private void WriteApplicationInfo()
        {
            ApplicationMetadata metadata = _cliContext.Metadata;

            // Title and version
            Write(TitleColor, metadata.Title);
            Write(' ');
            Write(VersionColor, metadata.VersionText);
            WriteLine();

            // Description
            if (!string.IsNullOrWhiteSpace(metadata.Description))
            {
                WriteHorizontalMargin();
                Write(metadata.Description);
                WriteLine();
            }
        }

        private void WriteDirectivesManual(IReadOnlyDictionary<string, DirectiveSchema> directives)
        {
            if (directives.Count == 0)
                return;

            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Directives");

            foreach (KeyValuePair<string, DirectiveSchema> directive in directives.OrderBy(x => x.Value.Name))
            {
                DirectiveSchema schema = directive.Value;

                // Name
                if (schema.InteractiveModeOnly)
                {
                    Write(InteractiveOnlyColor, "@");
                    WriteHorizontalMargin(1);
                }
                else
                    WriteHorizontalMargin();

                Write(DirectiveNameColor, "[");
                Write(DirectiveNameColor, directive.Key);
                Write(DirectiveNameColor, "]");
                WriteDirectiveDescription(schema);
                WriteLine();
            }
        }

        private void WriteDirectiveDescription(DirectiveSchema directive)
        {
            if (string.IsNullOrWhiteSpace(directive.Description))
                return;

            WriteColumnMargin();
            Write(directive.Description);
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

        private void WriteCommandManual(CommandSchema command)
        {
            if (string.IsNullOrWhiteSpace(command.Manual))
                return;

            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Manual");
            WriteHorizontalMargin();

            string sanitizedManual = command.Manual.Replace("\t", "  ");
            sanitizedManual = Regex.Replace(sanitizedManual, @"\t\n\r", Environment.NewLine);

            Write(sanitizedManual);

            WriteLine();
        }

        private void WriteCommandUsage(IReadOnlyDictionary<string, DirectiveSchema> directives,
                                       CommandSchema command,
                                       IReadOnlyList<CommandSchema> childCommands)
        {
            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Usage");

            // Exe name
            if (command.InteractiveModeOnly)
            {
                Write(InteractiveOnlyColor, "@");
                WriteHorizontalMargin(1);
            }
            else
                WriteHorizontalMargin();

            if (!command.InteractiveModeOnly && !_cliContext.IsInteractiveMode)
                Write(_cliContext.Metadata.ExecutableName);

            // Child command placeholder
            if (directives.Any())
            {
                Write(' ');
                Write(DirectiveNameColor, "[directive]");
            }

            // Command name
            if (!string.IsNullOrWhiteSpace(command.Name))
            {
                Write(' ');
                Write(CommandNameColor, command.Name);
            }

            // Child command placeholder
            if (childCommands.Any())
            {
                Write(' ');
                Write(CommandNameColor, "[command]");
            }

            // Parameters
            foreach (CommandParameterSchema parameter in command.Parameters)
            {
                Write(' ');
                Write(parameter.IsScalar
                    ? $"<{parameter.Name}>"
                    : $"<{parameter.Name}...>"
                );
            }

            // Required options
            foreach (CommandOptionSchema option in command.Options.Where(o => o.IsRequired))
            {
                Write(' ');
                Write(ParametersColor, !string.IsNullOrWhiteSpace(option.Name)
                    ? $"--{option.Name}"
                    : $"-{option.ShortName}"
                );

                Write(' ');
                Write(option.IsScalar
                    ? "<value>"
                    : "<values...>"
                );
            }

            // Options placeholder
            Write(' ');
            Write(OptionsPlaceholderColor, "[options]");

            WriteLine();
        }

        private void WriteCommandParameters(CommandSchema command)
        {
            if (!command.Parameters.Any())
                return;

            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Parameters");

            foreach (CommandParameterSchema parameter in command.Parameters.OrderBy(p => p.Order))
            {
                Write(RequiredColor, "* ");
                Write(RequiredParameterNameColor, $"{parameter.Name}");

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

        private void WriteCommandOptions(CommandSchema command,
                                         IReadOnlyDictionary<ArgumentSchema, object?> argumentDefaultValues)
        {
            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Options");

            foreach (CommandOptionSchema option in command.Options.OrderByDescending(o => o.IsRequired))
            {
                if (option.IsRequired)
                {
                    Write(RequiredColor, "* ");
                }
                else
                {
                    WriteHorizontalMargin();
                }

                // Short name
                if (option.ShortName != null)
                {
                    Write(OptionNameColor, $"-{option.ShortName}");
                }

                // Separator
                if (!string.IsNullOrWhiteSpace(option.Name) && option.ShortName != null)
                {
                    Write('|');
                }

                // Name
                if (!string.IsNullOrWhiteSpace(option.Name))
                {
                    Write(OptionNameColor, $"--{option.Name}");
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
                    object? defaultValue = argumentDefaultValues.GetValueOrDefault(option);
                    string? defaultValueFormatted = FormatDefaultValue(defaultValue);
                    if (defaultValueFormatted != null)
                    {
                        Write($"Default: {defaultValueFormatted}.");
                    }
                }

                WriteLine();
            }
        }

        private void WriteCommandChildren(CommandSchema command,
                                          IReadOnlyList<CommandSchema> childCommands)
        {
            if (!childCommands.Any())
                return;

            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Commands");

            foreach (CommandSchema childCommand in childCommands)
            {
                string relativeCommandName = !string.IsNullOrWhiteSpace(command.Name)
                    ? childCommand.Name!.Substring(command.Name.Length).Trim()
                    : childCommand.Name!;

                // Name
                if (childCommand.InteractiveModeOnly)
                {
                    Write(InteractiveOnlyColor, "@");
                    WriteHorizontalMargin(1);
                }
                else
                    WriteHorizontalMargin();

                Write(CommandNameColor, relativeCommandName);

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

            bool isNormalMode = !command.InteractiveModeOnly && !_cliContext.IsInteractiveMode;
            if (isNormalMode)
                Write(_cliContext.Metadata.ExecutableName);

            if (!string.IsNullOrWhiteSpace(command.Name))
            {
                if (isNormalMode)
                    Write(' ');

                Write(ConsoleColor.Cyan, command.Name);
            }

            if (isNormalMode)
                Write(' ');

            Write(ConsoleColor.Cyan, "[command]");

            Write(' ');
            Write(ConsoleColor.White, "--help");

            Write("` to show help on a specific command.");

            WriteLine();
        }

        public void Write(RootSchema root,
                          CommandSchema command,
                          IReadOnlyDictionary<ArgumentSchema, object?> defaultValues)
        {
            IReadOnlyList<CommandSchema> childCommands = root.GetChildCommands(command.Name);

            _console.ResetColor();
            _console.ForegroundColor = ConsoleColor.Gray;

            if (command.IsDefault)
                WriteApplicationInfo();

            WriteCommandDescription(command);
            WriteCommandUsage(root.Directives, command, childCommands);
            WriteCommandParameters(command);
            WriteCommandOptions(command, defaultValues);
            WriteCommandChildren(command, childCommands);
            WriteDirectivesManual(root.Directives);
            WriteCommandManual(command);
        }
    }

    internal partial class HelpTextWriter
    {
        private static string FormatValidValues(IReadOnlyList<string> values)
        {
            return values.Select(v => v.Quote()).JoinToString(", ");
        }

        private static string? FormatDefaultValue(object? defaultValue)
        {
            if (defaultValue == null)
                return null;

            // Enumerable
            if (!(defaultValue is string) && defaultValue is IEnumerable defaultValues)
            {
                Type elementType = defaultValues.GetType().GetEnumerableUnderlyingType() ?? typeof(object);

                // If the ToString() method is not overriden, the default value can't be formatted nicely
                if (!elementType.IsToStringOverriden())
                    return null;

                return defaultValues.Cast<object?>()
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