using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CliFx.Utils;

// Adapted from:
// https://github.com/dotnet/runtime/blob/01b7e73cd378145264a7cb7a09365b41ed42b240/src/libraries/Common/src/System/Text/ConsoleEncoding.cs
// Also see:
// https://source.dot.net/#System.Console/ConsoleEncoding.cs,5eedd083a4a4f4a2
// Majority of overrides are just proxy calls to avoid potentially more expensive base behavior.
// The important part is the GetPreamble() method that has been overriden to return an empty array.
internal class NoPreambleEncoding : Encoding
{
    private readonly Encoding _underlyingEncoding;

    [ExcludeFromCodeCoverage]
    public override string EncodingName => _underlyingEncoding.EncodingName;

    [ExcludeFromCodeCoverage]
    public override string BodyName => _underlyingEncoding.BodyName;

    [ExcludeFromCodeCoverage]
    public override int CodePage => _underlyingEncoding.CodePage;

    [ExcludeFromCodeCoverage]
    public override int WindowsCodePage => _underlyingEncoding.WindowsCodePage;

    [ExcludeFromCodeCoverage]
    public override string HeaderName => _underlyingEncoding.HeaderName;

    [ExcludeFromCodeCoverage]
    public override string WebName => _underlyingEncoding.WebName;

    [ExcludeFromCodeCoverage]
    public override bool IsBrowserDisplay => _underlyingEncoding.IsBrowserDisplay;

    [ExcludeFromCodeCoverage]
    public override bool IsBrowserSave => _underlyingEncoding.IsBrowserSave;

    [ExcludeFromCodeCoverage]
    public override bool IsSingleByte => _underlyingEncoding.IsSingleByte;

    [ExcludeFromCodeCoverage]
    public override bool IsMailNewsDisplay => _underlyingEncoding.IsMailNewsDisplay;

    [ExcludeFromCodeCoverage]
    public override bool IsMailNewsSave => _underlyingEncoding.IsMailNewsSave;

    public NoPreambleEncoding(Encoding underlyingEncoding)
        : base(
            underlyingEncoding.CodePage,
            underlyingEncoding.EncoderFallback,
            underlyingEncoding.DecoderFallback
        )
    {
        _underlyingEncoding = underlyingEncoding;
    }

    // This is the only part that changes
    public override byte[] GetPreamble() => Array.Empty<byte>();

    [ExcludeFromCodeCoverage]
    public override int GetByteCount(char[] chars, int index, int count) =>
        _underlyingEncoding.GetByteCount(chars, index, count);

    [ExcludeFromCodeCoverage]
    public override int GetByteCount(char[] chars) => _underlyingEncoding.GetByteCount(chars);

    [ExcludeFromCodeCoverage]
    public override int GetByteCount(string s) => _underlyingEncoding.GetByteCount(s);

    [ExcludeFromCodeCoverage]
    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) =>
        _underlyingEncoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);

    [ExcludeFromCodeCoverage]
    public override byte[] GetBytes(char[] chars, int index, int count) =>
        _underlyingEncoding.GetBytes(chars, index, count);

    [ExcludeFromCodeCoverage]
    public override byte[] GetBytes(char[] chars) => _underlyingEncoding.GetBytes(chars);

    [ExcludeFromCodeCoverage]
    public override int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex) =>
        _underlyingEncoding.GetBytes(s, charIndex, charCount, bytes, byteIndex);

    [ExcludeFromCodeCoverage]
    public override byte[] GetBytes(string s) => _underlyingEncoding.GetBytes(s);

    [ExcludeFromCodeCoverage]
    public override int GetCharCount(byte[] bytes, int index, int count) =>
        _underlyingEncoding.GetCharCount(bytes, index, count);

    [ExcludeFromCodeCoverage]
    public override int GetCharCount(byte[] bytes) => _underlyingEncoding.GetCharCount(bytes);

    [ExcludeFromCodeCoverage]
    public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) =>
        _underlyingEncoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);

    [ExcludeFromCodeCoverage]
    public override char[] GetChars(byte[] bytes) => _underlyingEncoding.GetChars(bytes);

    [ExcludeFromCodeCoverage]
    public override char[] GetChars(byte[] bytes, int index, int count) =>
        _underlyingEncoding.GetChars(bytes, index, count);

    [ExcludeFromCodeCoverage]
    public override string GetString(byte[] bytes) => _underlyingEncoding.GetString(bytes);

    [ExcludeFromCodeCoverage]
    public override string GetString(byte[] bytes, int index, int count) =>
        _underlyingEncoding.GetString(bytes, index, count);

    [ExcludeFromCodeCoverage]
    public override int GetMaxByteCount(int charCount) =>
        _underlyingEncoding.GetMaxByteCount(charCount);

    [ExcludeFromCodeCoverage]
    public override int GetMaxCharCount(int byteCount) =>
        _underlyingEncoding.GetMaxCharCount(byteCount);

    [ExcludeFromCodeCoverage]
    public override bool IsAlwaysNormalized(NormalizationForm form) => _underlyingEncoding.IsAlwaysNormalized(form);

    [ExcludeFromCodeCoverage]
    public override Encoder GetEncoder() => _underlyingEncoding.GetEncoder();

    [ExcludeFromCodeCoverage]
    public override Decoder GetDecoder() => _underlyingEncoding.GetDecoder();

    [ExcludeFromCodeCoverage]
    public override object Clone() => new NoPreambleEncoding((Encoding) base.Clone());
}

internal static class NoPreambleEncodingExtensions
{
    public static Encoding WithoutPreamble(this Encoding encoding) =>
        encoding.GetPreamble().Length > 0
            ? new NoPreambleEncoding(encoding)
            : encoding;
}