using System;

namespace CliFx.Domain
{
    internal class CommandDirectiveInput
    {
        public string Name { get; }

        public bool IsDebugDirective => string.Equals(Name, "debug", StringComparison.OrdinalIgnoreCase);

        public bool IsPreviewDirective => string.Equals(Name, "preview", StringComparison.OrdinalIgnoreCase);

        public CommandDirectiveInput(string name)
        {
            Name = name;
        }

        public override string ToString() => $"[{Name}]";
    }
}