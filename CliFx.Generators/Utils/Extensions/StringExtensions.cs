using System.Text;

namespace CliFx.Generators.Utils.Extensions;

internal static class StringExtensions
{
    extension(string str)
    {
        public string? NullIfWhiteSpace() => !string.IsNullOrWhiteSpace(str) ? str : null;

        public string ToKebabCase()
        {
            if (string.IsNullOrEmpty(str))
                return str;

            var buffer = new StringBuilder();
            var lastCh = default(char?);

            foreach (var ch in str)
            {
                if (char.IsUpper(ch))
                {
                    if (lastCh is not null && !char.IsUpper(lastCh.Value) && lastCh.Value != '-')
                        buffer.Append('-');

                    buffer.Append(char.ToLowerInvariant(ch));
                }
                else
                {
                    buffer.Append(ch);
                }

                lastCh = ch;
            }

            return buffer.ToString();
        }
    }
}
