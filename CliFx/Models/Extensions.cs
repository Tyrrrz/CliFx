using System;
using System.Linq;
using CliFx.Internal;

namespace CliFx.Models
{
    public static class Extensions
    {
        public static bool IsCommandSpecified(this CommandInput commandInput) => !commandInput.CommandName.IsNullOrWhiteSpace();

        public static bool IsHelpRequested(this CommandInput commandInput)
        {
            var firstOptionAlias = commandInput.Options.FirstOrDefault()?.Alias;

            return string.Equals(firstOptionAlias, "help", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(firstOptionAlias, "h", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(firstOptionAlias, "?", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsVersionRequested(this CommandInput commandInput)
        {
            var firstOptionAlias = commandInput.Options.FirstOrDefault()?.Alias;

            return string.Equals(firstOptionAlias, "version", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsDefault(this CommandSchema commandSchema) => commandSchema.Name.IsNullOrWhiteSpace();
    }
}