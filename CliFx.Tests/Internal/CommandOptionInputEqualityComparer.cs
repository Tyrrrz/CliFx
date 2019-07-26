using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Models;

namespace CliFx.Tests.Internal
{
    public partial class CommandOptionInputEqualityComparer : IEqualityComparer<CommandOptionInput>
    {
        /// <inheritdoc />
        public bool Equals(CommandOptionInput x, CommandOptionInput y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is null || y is null)
                return false;

            return StringComparer.OrdinalIgnoreCase.Equals(x.Alias, y.Alias) &&
                   x.Values.SequenceEqual(y.Values, StringComparer.Ordinal);
        }

        /// <inheritdoc />
        public int GetHashCode(CommandOptionInput obj) => throw new NotSupportedException();
    }

    public partial class CommandOptionInputEqualityComparer
    {
        public static CommandOptionInputEqualityComparer Instance { get; } = new CommandOptionInputEqualityComparer();
    }
}