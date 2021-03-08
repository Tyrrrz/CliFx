using System;
using System.IO;
using System.Threading;

namespace CliFx.Infrastructure
{
    /// <summary>
    /// Implementation of <see cref="IConsole"/> that uses fake standard input, output, and error streams
    /// backed by in-memory stores.
    /// </summary>
    /// <remarks>
    /// This implementation is designed for usage in tests.
    /// </remarks>
    public class FakeInMemoryConsole : IFakeConsole, IDisposable
    {
        private readonly MemoryStream _inputStream = new();
        private readonly MemoryStream _outputStream = new();
        private readonly MemoryStream _errorStream = new();

        private readonly IFakeConsole _innerConsole;

        /// <inheritdoc />
        public StreamReader Input => _innerConsole.Input;

        /// <inheritdoc />
        public bool IsInputRedirected => _innerConsole.IsInputRedirected;

        /// <inheritdoc />
        public StreamWriter Output => _innerConsole.Output;

        /// <inheritdoc />
        public bool IsOutputRedirected => _innerConsole.IsOutputRedirected;

        /// <inheritdoc />
        public StreamWriter Error => _innerConsole.Error;

        /// <inheritdoc />
        public bool IsErrorRedirected => _innerConsole.IsErrorRedirected;

        /// <inheritdoc />
        public ConsoleColor ForegroundColor
        {
            get => _innerConsole.ForegroundColor;
            set => _innerConsole.ForegroundColor = value;
        }

        /// <inheritdoc />
        public ConsoleColor BackgroundColor
        {
            get => _innerConsole.BackgroundColor;
            set => _innerConsole.BackgroundColor = value;
        }

        /// <inheritdoc />
        public void ResetColor()
        {
            _innerConsole.ResetColor();
        }

        /// <inheritdoc />
        public int CursorLeft
        {
            get => _innerConsole.CursorLeft;
            set => _innerConsole.CursorLeft = value;
        }

        /// <inheritdoc />
        public int CursorTop
        {
            get => _innerConsole.CursorTop;
            set => _innerConsole.CursorTop = value;
        }

        /// <inheritdoc />
        public CancellationToken RegisterCancellation() => _innerConsole.RegisterCancellation();

        /// <summary>
        /// Initializes an instance of <see cref="FakeInMemoryConsole"/>.
        /// </summary>
        public FakeInMemoryConsole() =>
            _innerConsole = new FakeConsole(_inputStream, _outputStream, _errorStream);

        /// <summary>
        /// Writes data to the input stream.
        /// </summary>
        public void WriteInput(byte[] data)
        {
            // TODO: is this safe?
            var lastPosition = _inputStream.Position;

            _inputStream.Write(data);
            _inputStream.Flush();

            _inputStream.Position = lastPosition;
        }

        /// <summary>
        /// Writes data to the input stream.
        /// </summary>
        public void WriteInput(string data) => WriteInput(
            _innerConsole.Input.CurrentEncoding.GetBytes(data)
        );

        /// <summary>
        /// Reads the data written to the output stream.
        /// </summary>
        public byte[] ReadOutputBytes()
        {
            _outputStream.Flush();
            return _outputStream.ToArray();
        }

        /// <summary>
        /// Reads the data written to the output stream.
        /// </summary>
        public string ReadOutputString() => _innerConsole.Output.Encoding.GetString(ReadOutputBytes());

        /// <summary>
        /// Reads the data written to the error stream.
        /// </summary>
        public byte[] ReadErrorBytes()
        {
            _errorStream.Flush();
            return _errorStream.ToArray();
        }

        /// <summary>
        /// Reads the data written to the error stream.
        /// </summary>
        public string ReadErrorString() => _innerConsole.Error.Encoding.GetString(ReadErrorBytes());

        /// <inheritdoc />
        public void RequestCancellation(TimeSpan delay) => _innerConsole.RequestCancellation(delay);

        /// <inheritdoc />
        public void RequestCancellation() => _innerConsole.RequestCancellation();

        /// <inheritdoc />
        public void Dispose()
        {
            _inputStream.Dispose();
            _outputStream.Dispose();
            _errorStream.Dispose();
        }
    }
}