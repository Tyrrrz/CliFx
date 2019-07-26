using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Models;

namespace CliFx.Tests.Internal
{
    public partial class CommandSchemaEqualityComparer : IEqualityComparer<CommandSchema>
    {
        /// <inheritdoc />
        public bool Equals(CommandSchema x, CommandSchema y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is null || y is null)
                return false;

            return x.Type == y.Type &&
                   StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name) &&
                   StringComparer.Ordinal.Equals(x.Description, y.Description) &&
                   x.Options.SequenceEqual(y.Options, CommandOptionSchemaEqualityComparer.Instance);
        }

        /// <inheritdoc />
        public int GetHashCode(CommandSchema obj) => throw new NotSupportedException();
    }

    public partial class CommandSchemaEqualityComparer
    {
        public static CommandSchemaEqualityComparer Instance { get; } = new CommandSchemaEqualityComparer();
    }
}