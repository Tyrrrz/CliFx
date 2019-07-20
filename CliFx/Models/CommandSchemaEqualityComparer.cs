using System;
using System.Collections.Generic;
using System.Linq;
using CliFx.Internal;

namespace CliFx.Models
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
                   x.IsDefault == y.IsDefault &&
                   StringComparer.Ordinal.Equals(x.Description, y.Description) &&
                   x.Options.SequenceEqual(y.Options, CommandOptionSchemaEqualityComparer.Instance);
        }

        /// <inheritdoc />
        public int GetHashCode(CommandSchema obj) => new HashCodeBuilder()
            .Add(obj.Type)
            .Add(obj.Name, StringComparer.OrdinalIgnoreCase)
            .Add(obj.IsDefault)
            .Add(obj.Description, StringComparer.Ordinal)
            .AddMany(obj.Options, CommandOptionSchemaEqualityComparer.Instance)
            .Build();
    }

    public partial class CommandSchemaEqualityComparer
    {
        public static CommandSchemaEqualityComparer Instance { get; } = new CommandSchemaEqualityComparer();
    }
}