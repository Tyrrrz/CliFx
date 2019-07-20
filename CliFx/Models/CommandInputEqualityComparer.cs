using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Internal;

namespace CliFx.Models
{
    public partial class CommandInputEqualityComparer : IEqualityComparer<CommandInput>
    {
        /// <inheritdoc />
        public bool Equals(CommandInput x, CommandInput y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is null || y is null)
                return false;

            return StringComparer.OrdinalIgnoreCase.Equals(x.CommandName, y.CommandName) &&
                   x.Options.SequenceEqual(y.Options, CommandOptionInputEqualityComparer.Instance);
        }

        /// <inheritdoc />
        public int GetHashCode(CommandInput obj) => new HashCodeBuilder()
            .Add(obj.CommandName, StringComparer.OrdinalIgnoreCase)
            .AddMany(obj.Options, CommandOptionInputEqualityComparer.Instance)
            .Build();
    }

    public partial class CommandInputEqualityComparer
    {
        public static CommandInputEqualityComparer Instance { get; } = new CommandInputEqualityComparer();
    }
}