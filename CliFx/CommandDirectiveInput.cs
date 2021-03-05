using System;
using System.Diagnostics.CodeAnalysis;

namespace CliFx
{
    internal class CommandDirectiveInput
    {
        public string Name { get; }

        public bool IsDebugDirective => string.Equals(Name, "debug", StringComparison.OrdinalIgnoreCase);

        public bool IsPreviewDirective => string.Equals(Name, "preview", StringComparison.OrdinalIgnoreCase);

        public CommandDirectiveInput(string name) => Name = name;

        [ExcludeFromCodeCoverage]
        public override string ToString() => $"[{Name}]";
    }
}