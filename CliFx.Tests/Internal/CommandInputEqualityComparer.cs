using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Models;

namespace CliFx.Tests.Internal
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
        public int GetHashCode(CommandInput obj) => throw new NotSupportedException();
    }

    public partial class CommandInputEqualityComparer
    {
        public static CommandInputEqualityComparer Instance { get; } = new CommandInputEqualityComparer();
    }
}