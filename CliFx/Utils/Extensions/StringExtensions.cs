namespace CliFx.Utils.Extensions;

internal static class StringExtensions
{
    extension(char c)
    {
        public string Repeat(int count) => new(c, count);

        public string AsString() => c.Repeat(1);
    }
}
