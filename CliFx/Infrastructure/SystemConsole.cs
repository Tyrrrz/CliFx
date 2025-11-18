using System;
using System.Runtime.Versioning;
using System.Threading;

namespace CliFx.Infrastructure;

/// <summary>
/// Implementation of <see cref="IConsole" /> that represents the real system console.
/// </summary>
public class SystemConsole : IConsole, IDisposable
{
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Initializes an instance of <see cref="SystemConsole" />.
    /// </summary>
    public SystemConsole()
    {
        Input = new ConsoleReader(this, Console.OpenStandardInput());
        Output = new ConsoleWriter(this, Console.OpenStandardOutput());
        Error = new ConsoleWriter(this, Console.OpenStandardError());
    }

    /// <inheritdoc />
    public ConsoleReader Input { get; }

    /// <inheritdoc />
    public bool IsInputRedirected => Console.IsInputRedirected;

    /// <inheritdoc />
    public ConsoleWriter Output { get; }

    /// <inheritdoc />
    public bool IsOutputRedirected => Console.IsOutputRedirected;

    /// <inheritdoc />
    public ConsoleWriter Error { get; }

    /// <inheritdoc />
    public bool IsErrorRedirected => Console.IsErrorRedirected;

    /// <inheritdoc />
    public ConsoleColor ForegroundColor
    {
        get => Console.ForegroundColor;
        set => Console.ForegroundColor = value;
    }

    /// <inheritdoc />
    public ConsoleColor BackgroundColor
    {
        get => Console.BackgroundColor;
        set => Console.BackgroundColor = value;
    }

    /// <inheritdoc />
    [SupportedOSPlatform("windows")]
    public int WindowWidth
    {
        get => Console.WindowWidth;
#pragma warning disable CA1416
        set => Console.WindowWidth = value;
#pragma warning restore CA1416
    }

    /// <inheritdoc />
    [SupportedOSPlatform("windows")]
    public int WindowHeight
    {
        get => Console.WindowHeight;
#pragma warning disable CA1416
        set => Console.WindowHeight = value;
#pragma warning restore CA1416
    }

    /// <inheritdoc />
    public int CursorLeft
    {
        get => Console.CursorLeft;
        set => Console.CursorLeft = value;
    }

    /// <inheritdoc />
    public int CursorTop
    {
        get => Console.CursorTop;
        set => Console.CursorTop = value;
    }

    /// <inheritdoc />
    public ConsoleKeyInfo ReadKey(bool intercept = false) => Console.ReadKey(intercept);

    /// <inheritdoc />
    public void ResetColor() => Console.ResetColor();

    /// <inheritdoc />
    public void Clear() => Console.Clear();

    /// <inheritdoc />
    public CancellationToken RegisterCancellationHandler()
    {
        if (_cancellationTokenSource is not null)
            return _cancellationTokenSource.Token;

        var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, args) =>
        {
            // Don't delay cancellation more than once
            if (!cts.IsCancellationRequested)
            {
                args.Cancel = true;
                cts.Cancel();
            }
        };

        return (_cancellationTokenSource = cts).Token;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();

        Input.Dispose();
        Output.Dispose();
        Error.Dispose();
    }
}
