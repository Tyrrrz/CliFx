using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CliFx.Utils;

// Adapted from:
// https://github.com/dotnet/runtime/blob/01b7e73cd378145264a7cb7a09365b41ed42b240/src/libraries/Common/src/System/Text/ConsoleEncoding.cs
// Also see:
// https://source.dot.net/#System.Console/ConsoleEncoding.cs,5eedd083a4a4f4a2
// Majority of overrides are just proxy calls to avoid potentially more expensive base behavior.
// The important part is the GetPreamble() method that has been overriden to return an empty array.
internal class NoPreambleEncoding(Encoding underlyingEncoding)
    : Encoding(
        underlyingEncoding.CodePage,
        underlyingEncoding.EncoderFallback,
        underlyingEncoding.DecoderFallback
    )
{
    [ExcludeFromCodeCoverage]
    public override string EncodingName => underlyingEncoding.EncodingName;

    [ExcludeFromCodeCoverage]
    public override string BodyName => underlyingEncoding.BodyName;

    [ExcludeFromCodeCoverage]
    public override int CodePage => underlyingEncoding.CodePage;

    [ExcludeFromCodeCoverage]
    public override int WindowsCodePage => underlyingEncoding.WindowsCodePage;

    [ExcludeFromCodeCoverage]
    public override string HeaderName => underlyingEncoding.HeaderName;

    [ExcludeFromCodeCoverage]
    public override string WebName => underlyingEncoding.WebName;

    [ExcludeFromCodeCoverage]
    public override bool IsBrowserDisplay => underlyingEncoding.IsBrowserDisplay;

    [ExcludeFromCodeCoverage]
    public override bool IsBrowserSave => underlyingEncoding.IsBrowserSave;

    [ExcludeFromCodeCoverage]
    public override bool IsSingleByte => underlyingEncoding.IsSingleByte;

    [ExcludeFromCodeCoverage]
    public override bool IsMailNewsDisplay => underlyingEncoding.IsMailNewsDisplay;

    [ExcludeFromCodeCoverage]
    public override bool IsMailNewsSave => underlyingEncoding.IsMailNewsSave;

    // This is the only part that changes
    public override byte[] GetPreamble() => [];

    [ExcludeFromCodeCoverage]
    public override int GetByteCount(char[] chars, int index, int count) =>
        underlyingEncoding.GetByteCount(chars, index, count);

    [ExcludeFromCodeCoverage]
    public override int GetByteCount(char[] chars) => underlyingEncoding.GetByteCount(chars);

    [ExcludeFromCodeCoverage]
    public override int GetByteCount(string s) => underlyingEncoding.GetByteCount(s);

    [ExcludeFromCodeCoverage]
    public override int GetBytes(
        char[] chars,
        int charIndex,
        int charCount,
        byte[] bytes,
        int byteIndex
    ) => underlyingEncoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);

    [ExcludeFromCodeCoverage]
    public override byte[] GetBytes(char[] chars, int index, int count) =>
        underlyingEncoding.GetBytes(chars, index, count);

    [ExcludeFromCodeCoverage]
    public override byte[] GetBytes(char[] chars) => underlyingEncoding.GetBytes(chars);

    [ExcludeFromCodeCoverage]
    public override int GetBytes(
        string s,
        int charIndex,
        int charCount,
        byte[] bytes,
        int byteIndex
    ) => underlyingEncoding.GetBytes(s, charIndex, charCount, bytes, byteIndex);

    [ExcludeFromCodeCoverage]
    public override byte[] GetBytes(string s) => underlyingEncoding.GetBytes(s);

    [ExcludeFromCodeCoverage]
    public override int GetCharCount(byte[] bytes, int index, int count) =>
        underlyingEncoding.GetCharCount(bytes, index, count);

    [ExcludeFromCodeCoverage]
    public override int GetCharCount(byte[] bytes) => underlyingEncoding.GetCharCount(bytes);

    [ExcludeFromCodeCoverage]
    public override int GetChars(
        byte[] bytes,
        int byteIndex,
        int byteCount,
        char[] chars,
        int charIndex
    ) => underlyingEncoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);

    [ExcludeFromCodeCoverage]
    public override char[] GetChars(byte[] bytes) => underlyingEncoding.GetChars(bytes);

    [ExcludeFromCodeCoverage]
    public override char[] GetChars(byte[] bytes, int index, int count) =>
        underlyingEncoding.GetChars(bytes, index, count);

    [ExcludeFromCodeCoverage]
    public override string GetString(byte[] bytes) => underlyingEncoding.GetString(bytes);

    [ExcludeFromCodeCoverage]
    public override string GetString(byte[] bytes, int index, int count) =>
        underlyingEncoding.GetString(bytes, index, count);

    [ExcludeFromCodeCoverage]
    public override int GetMaxByteCount(int charCount) =>
        underlyingEncoding.GetMaxByteCount(charCount);

    [ExcludeFromCodeCoverage]
    public override int GetMaxCharCount(int byteCount) =>
        underlyingEncoding.GetMaxCharCount(byteCount);

    [ExcludeFromCodeCoverage]
    public override bool IsAlwaysNormalized(NormalizationForm form) =>
        underlyingEncoding.IsAlwaysNormalized(form);

    [ExcludeFromCodeCoverage]
    public override Encoder GetEncoder() => underlyingEncoding.GetEncoder();

    [ExcludeFromCodeCoverage]
    public override Decoder GetDecoder() => underlyingEncoding.GetDecoder();

    [ExcludeFromCodeCoverage]
    public override object Clone() => new NoPreambleEncoding((Encoding)base.Clone());
}

internal static class NoPreambleEncodingExtensions
{
    extension(Encoding encoding)
    {
        public Encoding WithoutPreamble() =>
            encoding.GetPreamble().Length > 0 ? new NoPreambleEncoding(encoding) : encoding;
    }
}
