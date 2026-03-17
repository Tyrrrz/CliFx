using System.Text;

namespace CliFx.Utils;

internal static class NoPreambleEncodingExtensions
{
    extension(Encoding encoding)
    {
        public Encoding WithoutPreamble() =>
            encoding.GetPreamble().Length > 0 ? new NoPreambleEncoding(encoding) : encoding;
    }
}
