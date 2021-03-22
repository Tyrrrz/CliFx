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

        private void WriteHeader(string text)
        {
            Write(ConsoleColor.White, text.ToUpperInvariant());
            WriteLine();
        }

        private void WriteCommandInvocation()
        {
            Write(ConsoleColor.DarkGray, _context.ApplicationMetadata.ExecutableName);

            // Command name
            if (!string.IsNullOrWhiteSpace(_context.CommandSchema.Name))
            {
                Write(' ');
                Write(ConsoleColor.Cyan, _context.CommandSchema.Name);
            }
        }

        private void WriteApplicationInfo()
        {
            if (!IsEmpty)
                WriteVerticalMargin();

            // Title and version
            Write(ConsoleColor.White, _context.ApplicationMetadata.Title);
            Write(' ');
            Write(ConsoleColor.Yellow, _context.ApplicationMetadata.Version);
            WriteLine();

            // Description
            if (!string.IsNullOrWhiteSpace(_context.ApplicationMetadata.Description))
            {
                WriteHorizontalMargin();
                Write(_context.ApplicationMetadata.Description);
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
                foreach (var parameter in _context.CommandSchema.Parameters)
                {
                    Write(ConsoleColor.DarkCyan, parameter.Property.IsScalar()
                        ? $"<{parameter.Name}>"
                        : $"<{parameter.Name}...>"
                    );
                    Write(' ');
                }

                // Required options
                foreach (var option in _context.CommandSchema.Options.Where(o => o.IsRequired))
                {
                    Write(ConsoleColor.Yellow, !string.IsNullOrWhiteSpace(option.Name)
                        ? $"--{option.Name}"
                        : $"-{option.ShortName}"
                    );
                    Write(' ');

                    Write(ConsoleColor.White, option.Property.IsScalar()
                        ? "<value>"
                        : "<values...>"
                    );
                    Write(' ');
                }

                // Placeholder for non-required options
                if (_context.CommandSchema.Options.Any(o => !o.IsRequired))
                {
                    Write(ConsoleColor.Yellow, "[options]");
                }

                WriteLine();
            }

            // Child command usage
            var childCommandSchemas = _context
                .ApplicationSchema
                .GetChildCommands(_context.CommandSchema.Name);

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
            if (string.IsNullOrWhiteSpace(_context.CommandSchema.Description))
                return;

            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Description");

            WriteHorizontalMargin();

            Write(_context.CommandSchema.Description);
            WriteLine();
        }

        private void WriteCommandParameters()
        {
            if (!_context.CommandSchema.Parameters.Any())
                return;

            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Parameters");

            foreach (var parameterSchema in _context.CommandSchema.Parameters.OrderBy(p => p.Order))
            {
                Write(ConsoleColor.Red, "* ");
                Write(ConsoleColor.DarkCyan, $"{parameterSchema.Name}");

                WriteColumnMargin();

                // Description
                if (!string.IsNullOrWhiteSpace(parameterSchema.Description))
                {
                    Write(parameterSchema.Description);
                    Write(' ');
                }

                // Valid values
                var validValues = parameterSchema.Property.GetValidValues();
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

                        Write(ConsoleColor.DarkGray, '"');
                        Write(validValue.ToString());
                        Write(ConsoleColor.DarkGray, '"');
                    }

                    Write('.');
                    Write(' ');
                }

                WriteLine();
            }
        }

        private void WriteCommandOptions()
        {
            if (!IsEmpty)
                WriteVerticalMargin();

            WriteHeader("Options");

            foreach (var optionSchema in _context.CommandSchema.Options.OrderByDescending(o => o.IsRequired))
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
                var validValues = optionSchema.Property.GetValidValues();
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

                        Write(ConsoleColor.DarkGray, '"');
                        Write(validValue.ToString());
                        Write(ConsoleColor.DarkGray, '"');
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
                    var defaultValue = _context.CommandDefaultValues.GetValueOrDefault(optionSchema);
                    if (defaultValue is not null)
                    {
                        // Non-Scalar
                        if (defaultValue is not string && defaultValue is IEnumerable defaultValues)
                        {
                            var elementType =
                                defaultValues.GetType().TryGetEnumerableUnderlyingType() ??
                                typeof(object);

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

                                    Write(ConsoleColor.DarkGray, '"');
                                    Write(element.ToString(CultureInfo.InvariantCulture));
                                    Write(ConsoleColor.DarkGray, '"');
                                }
                            }
                        }
                        else
                        {
                            if (defaultValue.GetType().IsToStringOverriden())
                            {
                                Write(ConsoleColor.White, "Default: ");

                                Write(ConsoleColor.DarkGray, '"');
                                Write(defaultValue.ToString(CultureInfo.InvariantCulture));
                                Write(ConsoleColor.DarkGray, '"');
                            }
                        }

                        Write('.');
                    }
                }

                WriteLine();
            }
        }

        private void WriteCommandChildren()
        {
            var childCommandSchemas = _context
                .ApplicationSchema
                .GetChildCommands(_context.CommandSchema.Name);

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
                    childCommandSchema
                        .Name?
                        .Substring(_context.CommandSchema.Name?.Length ?? 0)
                        .Trim()
                );

                WriteColumnMargin();

                // Description
                if (!string.IsNullOrWhiteSpace(childCommandSchema.Description))
                {
                    Write(childCommandSchema.Description);
                    Write(' ');
                }

                // Child commands of child command
                var grandChildCommandSchemas = _context
                    .ApplicationSchema
                    .GetChildCommands(childCommandSchema.Name);

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
                                .Name?
                                .Substring(_context.CommandSchema.Name?.Length ?? 0)
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
        public static void WriteHelpText(this IConsole console, HelpContext context) =>
            new HelpConsoleFormatter(console.Output, context).WriteHelpText();
    }
}