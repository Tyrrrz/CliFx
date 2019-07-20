using System;
using System.Linq;
using CliFx.Internal;

namespace CliFx.Models
{
    public static class Extensions
    {
        public static CommandOptionInput GetOptionOrDefault(this CommandInput set, string name, char? shortName) =>
            set.Options.FirstOrDefault(o =>
            {
                if (!name.IsNullOrWhiteSpace() && string.Equals(o.Name, name, StringComparison.Ordinal))
                    return true;

                if (shortName != null && o.Name.Length == 1 && o.Name.Single() == shortName)
                    return true;

                return false;
            });
    }
}