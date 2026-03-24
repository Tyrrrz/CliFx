using System.Text;

namespace CliFx.Utils.Extensions;

internal static class EncodingExtensions
{
    extension(Encoding encoding)
    {
        public Encoding WithoutPreamble() =>
            encoding.GetPreamble().Length > 0 ? new NoPreambleEncoding(encoding) : encoding;
    }
}
