using System.Collections.Generic;

namespace CliFx.Domain
{
    internal class ResolvedCommand
    {
        public ICommand Instance { get; }

        public CommandSchema Schema { get; }

        public IReadOnlyList<CommandSchema> ChildCommandSchemas { get; }

        public IReadOnlyDictionary<CommandArgumentSchema, object?> DefaultValues { get; }

        public IReadOnlyList<string> Directives { get; }

        public bool ShowHelp { get; }

        public bool ShowVersion { get; }

        public ResolvedCommand(
            ICommand instance,
            CommandSchema schema,
            IReadOnlyList<CommandSchema> childCommandSchemas,
            IReadOnlyDictionary<CommandArgumentSchema, object?> defaultValues,
            IReadOnlyList<string> directives,
            bool showHelp,
            bool showVersion)
        {
            Instance = instance;
            Schema = schema;
            ChildCommandSchemas = childCommandSchemas;
            DefaultValues = defaultValues;
            Directives = directives;
            ShowHelp = showHelp;
            ShowVersion = showVersion;
        }
    }
}