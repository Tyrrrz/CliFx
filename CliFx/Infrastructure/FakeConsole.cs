using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace CliFx.Infrastructure;

/// <summary>
/// Implementation of <see cref="IConsole" /> that uses the provided fake
/// standard input, output, and error streams.
/// </summary>
/// <remarks>
/// Use this implementation in tests to verify how a command interacts with the console.
/// </remarks>
public class FakeConsole : IConsole, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ConcurrentQueue<ConsoleKeyInfo> _keys = new();

    /// <inheritdoc />
    public ConsoleReader Input { get; }

    /// <inheritdoc />
    public bool IsInputRedirected => true;

    /// <inheritdoc />
    public ConsoleWriter Output { get; }

    /// <inheritdoc />
    public bool IsOutputRedirected => true;

    /// <inheritdoc />
    public ConsoleWriter Error { get; }

    /// <inheritdoc />
    public bool IsErrorRedirected => true;

    /// <inheritdoc />
    public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Gray;

    /// <inheritdoc />
    public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;

    /// <inheritdoc />
    public int WindowWidth { get; set; } = 232; // Windows defaults

    /// <inheritdoc />
    public int WindowHeight { get; set; } = 14; // Windows defaults

    /// <inheritdoc />
    public int CursorLeft { get; set; }

    /// <inheritdoc />
    public int CursorTop { get; set; }

    /// <summary>
    /// Initializes an instance of <see cref="FakeConsole" />.
    /// </summary>
    public FakeConsole(Stream? input = null, Stream? output = null, Stream? error = null)
    {
        Input = ConsoleReader.Create(this, input ?? Stream.Null);
        Output = ConsoleWriter.Create(this, output ?? Stream.Null);
        Error = ConsoleWriter.Create(this, error ?? Stream.Null);
    }

    /// <inheritdoc />
    public ConsoleKeyInfo ReadKey(bool intercept = false) =>
        _keys.TryDequeue(out var key)
            ? key
            : throw new InvalidOperationException(
                "Cannot read key because there are no key presses enqueued. " +
                $"Use the `{nameof(EnqueueKey)}(...)` method to simulate a key press."
            );

    /// <summary>
    /// Enqueues a simulated key press, which can then be read by calling <see cref="ReadKey" />.
    /// </summary>
    public void EnqueueKey(ConsoleKeyInfo key) => _keys.Enqueue(key);

    /// <inheritdoc />
    public void ResetColor()
    {
        ForegroundColor = ConsoleColor.Gray;
        BackgroundColor = ConsoleColor.Black;
    }

    /// <inheritdoc />
    public void Clear()
    {
    }

    /// <inheritdoc />
    public CancellationToken RegisterCancellationHandler() => _cancellationTokenSource.Token;

    /// <summary>
    /// Sends a cancellation signal to the currently executing command.
    /// </summary>
    /// <remarks>
    /// If the command is not cancellation-aware (i.e. it doesn't call <see cref="IConsole.RegisterCancellationHandler" />),
    /// this method will not have any effect.
    /// </remarks>
    public void RequestCancellation(TimeSpan? delay = null)
    {
        // Avoid unnecessary creation of a timer
        if (delay is not null && delay > TimeSpan.Zero)
        {
            _cancellationTokenSource.CancelAfter(delay.Value);
        }
        else
        {
            _cancellationTokenSource.Cancel();
        }
    }

    /// <inheritdoc />
    public virtual void Dispose() => _cancellationTokenSource.Dispose();
}