using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CliFx.Internal;
using CliFx.Models;

namespace CliFx.Services
{
    public partial class CommandHelpTextRenderer : ICommandHelpTextRenderer
    {
        private readonly IConsole _console;

        public CommandHelpTextRenderer(IConsole console)
        {
            _console = console;
        }

        public CommandHelpTextRenderer()
            : this(new SystemConsole())
        {
        }

        private void RenderDescription(CommandSchema commandSchema)
        {
            if (commandSchema.Description.IsNullOrWhiteSpace())
                return;

            _console.WithColor(ConsoleColor.Black, ConsoleColor.DarkGray, c =>
            {
                c.Output.WriteLine("Description");
            });

            _console.Output.Write("  ");
            _console.Output.Write(commandSchema.Description);
            _console.Output.WriteLine();

            _console.Output.WriteLine();
        }

        private void RenderUsage(ApplicationMetadata applicationMetadata, CommandSchema commandSchema, bool hasChildCommands)
        {
            _console.WithColor(ConsoleColor.Black, ConsoleColor.DarkGray, c =>
            {
                c.Output.WriteLine("Usage");
            });

            _console.Output.Write("  ");

            _console.Output.Write(applicationMetadata.ExecutableName);
            _console.Output.Write(' ');

            if (!commandSchema.IsDefault())
            {
                _console.Output.Write(commandSchema.Name);
                _console.Output.Write(' ');
            }

            if (hasChildCommands)
            {
                _console.Output.Write("[command]");
                _console.Output.Write(' ');
            }

            _console.Output.Write("[options]");
            _console.Output.WriteLine();
            _console.Output.WriteLine();
        }

        private void RenderOptions(CommandSchema commandSchema)
        {
            var options = new List<CommandOptionSchema>();
            options.AddRange(commandSchema.Options);

            options.Add(new CommandOptionSchema(null, "help", 'h', null, false, "Shows help text."));

            if (commandSchema.IsDefault())
                options.Add(new CommandOptionSchema(null, "version", 'v', null, false, "Shows application version."));

            _console.WithColor(ConsoleColor.Black, ConsoleColor.DarkGray, c =>
            {
                c.Output.WriteLine("Options");
            });

            foreach (var option in options)
            {
                _console.Output.Write("  ");

                if (!option.Name.IsNullOrWhiteSpace())
                {
                    _console.WithColor(option.IsRequired ? ConsoleColor.White : ConsoleColor.Gray, c =>
                    {
                        c.Output.Write("--");
                        c.Output.Write(option.Name);
                    });
                }

                if (!option.Name.IsNullOrWhiteSpace() && option.ShortName != null)
                {
                    _console.Output.Write('|');
                }

                if (option.ShortName != null)
                {
                    _console.WithColor(option.IsRequired ? ConsoleColor.White: ConsoleColor.Gray, c =>
                    {
                        c.Output.Write('-');
                        c.Output.Write(option.ShortName);
                    });
                }

                if (!option.Description.IsNullOrWhiteSpace())
                {
                    _console.Output.Write("    ");
                    _console.Output.Write(option.Description);
                }

                _console.Output.WriteLine();
            }

            _console.Output.WriteLine();
        }

        private void RenderChildCommands(ApplicationMetadata applicationMetadata, CommandSchema commandSchema,
            IReadOnlyList<CommandSchema> childCommandSchemas)
        {
            if (!childCommandSchemas.Any())
                return;

            _console.WithColor(ConsoleColor.Black, ConsoleColor.DarkGray, c =>
            {
                c.Output.WriteLine("Commands");
            });

            foreach (var childCommandSchema in childCommandSchemas)
            {
                _console.Output.Write("  ");

                _console.Output.Write(GetRelativeCommandName(childCommandSchema, commandSchema));

                if (!childCommandSchema.Description.IsNullOrWhiteSpace())
                {
                    _console.Output.Write("    ");
                    _console.Output.Write(childCommandSchema.Description);
                }

                _console.Output.WriteLine();
            }

            _console.Output.WriteLine();

            // Child command help tip

            _console.Output.Write("You can run `");

            _console.Output.Write(applicationMetadata.ExecutableName);
            _console.Output.Write(' ');

            if (!commandSchema.IsDefault())
            {
                _console.Output.Write(commandSchema.Name);
                _console.Output.Write(' ');
            }

            _console.Output.Write("[command] --help` to show help on a specific command.");

            _console.Output.WriteLine();
        }

        public void RenderHelpText(ApplicationMetadata applicationMetadata,
            IReadOnlyList<CommandSchema> availableCommandSchemas, CommandSchema matchingCommandSchema)
        {
            var childCommandSchemas = GetChildCommandSchemas(availableCommandSchemas, matchingCommandSchema);

            // Render application info
            if (matchingCommandSchema.IsDefault())
            {
                _console.Output.Write(applicationMetadata.Title);
                _console.Output.Write(" v");
                _console.Output.Write(applicationMetadata.VersionText);
                _console.Output.WriteLine();
                _console.Output.WriteLine();
            }

            RenderDescription(matchingCommandSchema);
            RenderUsage(applicationMetadata, matchingCommandSchema, childCommandSchemas.Any());
            RenderOptions(matchingCommandSchema);
            RenderChildCommands(applicationMetadata, matchingCommandSchema, childCommandSchemas);
        }
    }

    public partial class CommandHelpTextRenderer
    {
        private static CommandSchema GetParentOrNull(CommandSchema commandSchema, IReadOnlyList<CommandSchema> availableCommandSchemas)
        {
            if (commandSchema.IsDefault())
                return null;

            var nameBuffer = commandSchema.Name;
            while (nameBuffer.Contains(' '))
            {
                nameBuffer = nameBuffer.SubstringUntilLast(" ");
                var parent = availableCommandSchemas.FirstOrDefault(c =>
                    string.Equals(c.Name, nameBuffer, StringComparison.OrdinalIgnoreCase));

                if (parent != null)
                    return parent;
            }

            return availableCommandSchemas.FirstOrDefault(c => c.IsDefault());
        }

        private static IReadOnlyList<CommandSchema> GetChildCommandSchemas(IReadOnlyList<CommandSchema> availableCommandSchemas,
            CommandSchema parentCommandSchema)
        {
            var result = new List<CommandSchema>();

            foreach (var commandSchema in availableCommandSchemas)
            {
                if (commandSchema == parentCommandSchema)
                    continue;

                if (GetParentOrNull(commandSchema, availableCommandSchemas) == parentCommandSchema)
                    result.Add(commandSchema);
            }

            return result;
        }

        private static string GetRelativeCommandName(CommandSchema commandSchema, CommandSchema parentCommandSchema)
        {
            if (parentCommandSchema.Name.IsNullOrWhiteSpace())
                return commandSchema.Name;

            return commandSchema.Name.Substring(parentCommandSchema.Name.Length + 1);
        }
    }
}