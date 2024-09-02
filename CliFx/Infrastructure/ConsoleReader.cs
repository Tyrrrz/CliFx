using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CliFx.Infrastructure;

/// <summary>
/// Implements a <see cref="StreamReader" /> for reading characters or binary data from a console stream.
/// </summary>
// Both the underlying stream AND the stream reader must be synchronized!
// https://github.com/Tyrrrz/CliFx/issues/123
public sealed class ConsoleReader(IConsole console, Stream stream, Encoding encoding)
    : StreamReader(Stream.Synchronized(stream), encoding, false, 4096)
{
    /// <summary>
    /// Initializes an instance of <see cref="ConsoleReader" />.
    /// </summary>
    public ConsoleReader(IConsole console, Stream stream)
        : this(console, stream, System.Console.InputEncoding) { }

    /// <summary>
    /// Console that owns this stream.
    /// </summary>
    public IConsole Console { get; } = console;

    // The following overrides are required to establish thread-safe behavior
    // in methods deriving from StreamReader.

    /// <inheritdoc />
    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.Synchronized)]
    public override int Peek() => base.Peek();

    /// <inheritdoc />
    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.Synchronized)]
    public override int Read() => base.Read();

    /// <inheritdoc />
    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.Synchronized)]
    public override int Read(char[] buffer, int index, int count) =>
        base.Read(buffer, index, count);

    /// <inheritdoc />
    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.Synchronized)]
    public override int ReadBlock(char[] buffer, int index, int count) =>
        base.ReadBlock(buffer, index, count);

    /// <inheritdoc />
    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.Synchronized)]
    public override string? ReadLine() => base.ReadLine();

    /// <inheritdoc />
    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.Synchronized)]
    public override string ReadToEnd() => base.ReadToEnd();

    /// <inheritdoc />
    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.Synchronized)]
    public override Task<int> ReadAsync(char[] buffer, int index, int count) =>
        // Must be non-async to work with locks
        Task.FromResult(Read(buffer, index, count));

    /// <inheritdoc />
    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.Synchronized)]
    public override Task<int> ReadBlockAsync(char[] buffer, int index, int count) =>
        // Must be non-async to work with locks
        Task.FromResult(ReadBlock(buffer, index, count));

    /// <inheritdoc />
    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.Synchronized)]
    public override Task<string?> ReadLineAsync() =>
        // Must be non-async to work with locks
        Task.FromResult(ReadLine());

    /// <inheritdoc />
    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.Synchronized)]
    public override Task<string> ReadToEndAsync() =>
        // Must be non-async to work with locks
        Task.FromResult(ReadToEnd());

    /// <inheritdoc />
    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.Synchronized)]
    public override void Close() => base.Close();

    /// <inheritdoc />
    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.Synchronized)]
    protected override void Dispose(bool disposing) => base.Dispose(disposing);
}
