using System;
using System.Collections.Generic;
using CliFx.Models;

namespace CliFx.Tests.Internal
{
    public partial class CommandOptionSchemaEqualityComparer : IEqualityComparer<CommandOptionSchema>
    {
        /// <inheritdoc />
        public bool Equals(CommandOptionSchema x, CommandOptionSchema y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is null || y is null)
                return false;

            return x.Property == y.Property &&
                   StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name) &&
                   x.ShortName == y.ShortName &&
                   StringComparer.OrdinalIgnoreCase.Equals(x.GroupName, y.GroupName) &&
                   StringComparer.Ordinal.Equals(x.Description, y.Description);
        }

        /// <inheritdoc />
        public int GetHashCode(CommandOptionSchema obj) => throw new NotSupportedException();
    }

    public partial class CommandOptionSchemaEqualityComparer
    {
        public static CommandOptionSchemaEqualityComparer Instance { get; } = new CommandOptionSchemaEqualityComparer();
    }
}