using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Internal;

namespace CliFx.Models
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
        public int GetHashCode(CommandOptionInput obj) => new HashCodeBuilder()
            .Add(obj.Alias, StringComparer.OrdinalIgnoreCase)
            .AddMany(obj.Values, StringComparer.Ordinal)
            .Build();
    }

    public partial class CommandOptionInputEqualityComparer
    {
        public static CommandOptionInputEqualityComparer Instance { get; } = new CommandOptionInputEqualityComparer();
    }
}