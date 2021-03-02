using System;
using System.Diagnostics.CodeAnalysis;

namespace CliFx.Domain
{
    internal class CommandDirectiveInput
    {
        public string Name { get; }

        public bool IsDebugDirective => string.Equals(Name, "debug", StringComparison.OrdinalIgnoreCase);

        public bool IsPreviewDirective => string.Equals(Name, "preview", StringComparison.OrdinalIgnoreCase);

        public CommandDirectiveInput(string name) => Name = name;

        internal static bool IsDirective(string argument)
        {
            if (!argument.StartsWith('[') || !argument.EndsWith(']'))
            {
                return false;
            }

            return true;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() => $"[{Name}]";
    }
}