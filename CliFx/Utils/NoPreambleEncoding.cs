using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CliFx.Utils
{
    // Adapted from:
    // https://github.com/dotnet/runtime/blob/01b7e73cd378145264a7cb7a09365b41ed42b240/src/libraries/Common/src/System/Text/ConsoleEncoding.cs
    internal class NoPreambleEncoding : Encoding
    {
        private readonly Encoding _underlyingEncoding;

        public NoPreambleEncoding(Encoding underlyingEncoding) =>
            _underlyingEncoding = underlyingEncoding;

        public override byte[] GetPreamble() =>
            Array.Empty<byte>();

        [ExcludeFromCodeCoverage]
        public override int GetByteCount(char[] chars, int index, int count) =>
            _underlyingEncoding.GetByteCount(chars, index, count);

        [ExcludeFromCodeCoverage]
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) =>
            _underlyingEncoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);

        [ExcludeFromCodeCoverage]
        public override int GetCharCount(byte[] bytes, int index, int count) =>
            _underlyingEncoding.GetCharCount(bytes, index, count);

        [ExcludeFromCodeCoverage]
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) =>
            _underlyingEncoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);

        [ExcludeFromCodeCoverage]
        public override int GetMaxByteCount(int charCount) =>
            _underlyingEncoding.GetMaxByteCount(charCount);

        [ExcludeFromCodeCoverage]
        public override int GetMaxCharCount(int byteCount) =>
            _underlyingEncoding.GetMaxCharCount(byteCount);
    }

    internal static class NoPreambleEncodingExtensions
    {
        public static Encoding WithoutPreamble(this Encoding encoding) =>
            encoding.GetPreamble().Length > 0
                ? new NoPreambleEncoding(encoding)
                : encoding;
    }
}