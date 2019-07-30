using System;
using System.Collections.Generic;
using System.Linq;
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

        public void RenderHelpText(ApplicationMetadata applicationMetadata,
            IReadOnlyList<CommandSchema> availableCommandSchemas, CommandSchema matchingCommandSchema = null) =>
            new RenderHelpTextImpl(_console, applicationMetadata, availableCommandSchemas, matchingCommandSchema).Render();
    }

    public partial class CommandHelpTextRenderer
    {
        private class RenderHelpTextImpl
        {
            private readonly IConsole _console;
            private readonly ApplicationMetadata _applicationMetadata;
            private readonly IReadOnlyList<CommandSchema> _availableCommandSchemas;
            private readonly CommandSchema _matchingCommandSchema;

            private readonly IReadOnlyList<CommandSchema> _childCommandSchemas;

            public RenderHelpTextImpl(IConsole console, ApplicationMetadata applicationMetadata,
                IReadOnlyList<CommandSchema> availableCommandSchemas, CommandSchema matchingCommandSchema)
            {
                _console = console;
                _applicationMetadata = applicationMetadata;
                _availableCommandSchemas = availableCommandSchemas;
                _matchingCommandSchema = matchingCommandSchema;

                _childCommandSchemas = GetChildCommandSchemas();
            }

            private IReadOnlyList<CommandSchema> GetChildCommandSchemas()
            {
                // TODO:
                var prefix = _matchingCommandSchema == null || _matchingCommandSchema.Name.IsNullOrWhiteSpace()
                    ? ""
                    : _matchingCommandSchema.Name + " ";

                return new CommandSchema[0];
            }

            private void RenderAppInfo()
            {
                if (_matchingCommandSchema != null && !_matchingCommandSchema.Name.IsNullOrWhiteSpace())
                    return;

                _console.Output.Write(_applicationMetadata.Title);
                _console.Output.Write(" v");
                _console.Output.Write(_applicationMetadata.VersionText);
                _console.Output.WriteLine();
                _console.Output.WriteLine();
            }

            private void RenderDescription()
            {
                if (_matchingCommandSchema == null || _matchingCommandSchema.Description.IsNullOrWhiteSpace())
                    return;

                _console.WithColor(ConsoleColor.Black, ConsoleColor.DarkCyan, c =>
                {
                    c.Output.WriteLine("Description");
                });

                _console.Output.Write("  ");
                _console.Output.Write(_matchingCommandSchema.Description);
                _console.Output.WriteLine();

                _console.Output.WriteLine();
            }

            private void RenderUsage()
            {
                var hasChildCommands = _childCommandSchemas.Any();

                _console.WithColor(ConsoleColor.Black, ConsoleColor.DarkCyan, c =>
                {
                    c.Output.WriteLine("Usage");
                });

                _console.Output.Write("  ");

                _console.Output.Write(_applicationMetadata.ExecutableName);
                _console.Output.Write(' ');

                if (_matchingCommandSchema != null && !_matchingCommandSchema.Name.IsNullOrWhiteSpace())
                {
                    _console.Output.Write(_matchingCommandSchema.Name);
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

            private void RenderOptions()
            {
                var options = new List<CommandOptionSchema>();
                options.AddRange(_matchingCommandSchema?.Options ?? Enumerable.Empty<CommandOptionSchema>());
                options.Add(new CommandOptionSchema(null, "help", 'h', null, false, "Shows help text."));

                if (_matchingCommandSchema == null || _matchingCommandSchema.Name.IsNullOrWhiteSpace())
                    options.Add(new CommandOptionSchema(null, "version", 'v', null, false, "Shows application version."));

                _console.WithColor(ConsoleColor.Black, ConsoleColor.DarkCyan, c =>
                {
                    c.Output.WriteLine("Options");
                });

                foreach (var option in options)
                {
                    _console.Output.Write("  ");

                    if (!option.Name.IsNullOrWhiteSpace())
                    {
                        _console.WithColor(option.IsRequired ? ConsoleColor.Yellow : ConsoleColor.White, c =>
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
                        _console.WithColor(option.IsRequired ? ConsoleColor.Yellow : ConsoleColor.White, c =>
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

            private void RenderChildCommands()
            {
                // TODO
            }

            private void RenderSubCommandHelpTip()
            {
                if (!_childCommandSchemas.Any())
                    return;

                _console.Output.Write("You can run `");

                _console.Output.Write(_applicationMetadata.ExecutableName);
                _console.Output.Write(' ');

                if (_matchingCommandSchema != null && !_matchingCommandSchema.Name.IsNullOrWhiteSpace())
                {
                    _console.Output.Write(_matchingCommandSchema.Name);
                    _console.Output.Write(' ');
                }

                _console.Output.Write("[command] --help` to show help on a specific command.");

                _console.Output.WriteLine();
            }

            public void Render()
            {
                RenderAppInfo();
                RenderDescription();
                RenderUsage();
                RenderOptions();
                RenderChildCommands();
                RenderSubCommandHelpTip();
            }
        }
    }
}