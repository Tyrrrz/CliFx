using System;
using System.Collections.Generic;
using CliFx.Internal;

namespace CliFx.Models
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
        public int GetHashCode(CommandOptionSchema obj) => new HashCodeBuilder()
            .Add(obj.Property)
            .Add(obj.Name, StringComparer.OrdinalIgnoreCase)
            .Add(obj.ShortName)
            .Add(obj.GroupName, StringComparer.OrdinalIgnoreCase)
            .Add(obj.Description, StringComparer.Ordinal)
            .Build();
    }

    public partial class CommandOptionSchemaEqualityComparer
    {
        public static CommandOptionSchemaEqualityComparer Instance { get; } = new CommandOptionSchemaEqualityComparer();
    }
}