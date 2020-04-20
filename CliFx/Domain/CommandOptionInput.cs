using System.Collections.Generic;
using System.Text;
using CliFx.Internal;

namespace CliFx.Domain
{
    internal class CommandOptionInput
    {
        public string Alias { get; }

        public string DisplayAlias =>
            Alias.Length > 1
                ? $"--{Alias}"
                : $"-{Alias}";

        public IReadOnlyList<string> Values { get; }

        public bool IsHelpOption => CommandOptionSchema.HelpOption.MatchesNameOrShortName(Alias);

        public bool IsVersionOption => CommandOptionSchema.VersionOption.MatchesNameOrShortName(Alias);

        public CommandOptionInput(string alias, IReadOnlyList<string> values)
        {
            Alias = alias;
            Values = values;
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();

            buffer.Append(DisplayAlias);

            foreach (var value in Values)
            {
                buffer.AppendIfNotEmpty(' ');

                var isEscaped = value.Contains(" ");

                if (isEscaped)
                    buffer.Append('"');

                buffer.Append(value);

                if (isEscaped)
                    buffer.Append('"');
            }

            return buffer.ToString();
        }
    }
}