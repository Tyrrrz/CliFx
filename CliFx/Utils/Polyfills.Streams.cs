// ReSharper disable CheckNamespace

#if NETSTANDARD2_0
using System.IO;

internal static class StreamPolyfills
{
    public static void Write(this Stream stream, byte[] buffer) =>
        stream.Write(buffer, 0, buffer.Length);
}
#endif