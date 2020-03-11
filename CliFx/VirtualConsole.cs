using System;
using System.IO;
using System.Threading;

namespace CliFx
{
    /// <summary>
    /// Implementation of <see cref="IConsole"/> that routes data to specified streams.
    /// Does not leak to system console in any way.
    /// Use this class as a substitute for system console when running tests.
    /// </summary>
    public partial class VirtualConsole
    {
        private readonly MemoryStream _inputStream = new MemoryStream();
        private readonly MemoryStream _outputStream = new MemoryStream();
        private readonly MemoryStream _errorStream = new MemoryStream();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// Initializes an instance of <see cref="VirtualConsole"/>.
        /// </summary>
        public VirtualConsole(bool isRedirected)
        {
            Input = new StreamReader(_inputStream, Console.InputEncoding, false);
            Output = new StreamWriter(_outputStream, Console.OutputEncoding) {AutoFlush = true};
            Error = new StreamWriter(_errorStream, Console.OutputEncoding) {AutoFlush = true};

            IsInputRedirected = isRedirected;
            IsOutputRedirected = isRedirected;
            IsErrorRedirected = isRedirected;
        }

        /// <summary>
        /// Initializes an instance of <see cref="VirtualConsole"/>.
        /// </summary>
        public VirtualConsole()
            : this(true)
        {
        }

        /// <summary>
        /// Writes raw data to input stream.
        /// </summary>
        public void WriteInputData(byte[] data) => _inputStream.Write(data, 0, data.Length);

        /// <summary>
        /// Writes text to input stream.
        /// </summary>
        public void WriteInputString(string str) => WriteInputData(Input.CurrentEncoding.GetBytes(str));

        /// <summary>
        /// Reads all data written to output stream thus far.
        /// </summary>
        public byte[] ReadOutputData() => _outputStream.ToArray();

        /// <summary>
        /// Reads all text written to output stream thus far.
        /// </summary>
        public string ReadOutputString() => Output.Encoding.GetString(ReadOutputData());

        /// <summary>
        /// Reads all data written to error stream thus far.
        /// </summary>
        public byte[] ReadErrorData() => _errorStream.ToArray();

        /// <summary>
        /// Reads all text written to error stream thus far.
        /// </summary>
        public string ReadErrorString() => Error.Encoding.GetString(ReadErrorData());

        /// <summary>
        /// Sends an interrupt signal.
        /// </summary>
        public void Cancel() => _cts.Cancel();

        /// <summary>
        /// Sends an interrupt signal after a delay.
        /// </summary>
        public void CancelAfter(TimeSpan delay) => _cts.CancelAfter(delay);
    }

    public partial class VirtualConsole : IConsole
    {
        /// <inheritdoc />
        public StreamReader Input { get; }

        /// <inheritdoc />
        public bool IsInputRedirected { get; }

        /// <inheritdoc />
        public StreamWriter Output { get; }

        /// <inheritdoc />
        public bool IsOutputRedirected { get; }

        /// <inheritdoc />
        public StreamWriter Error { get; }

        /// <inheritdoc />
        public bool IsErrorRedirected { get; }

        /// <inheritdoc />
        public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Gray;

        /// <inheritdoc />
        public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;

        /// <inheritdoc />
        public void ResetColor()
        {
            ForegroundColor = ConsoleColor.Gray;
            BackgroundColor = ConsoleColor.Black;
        }

        /// <inheritdoc />
        public CancellationToken GetCancellationToken() => _cts.Token;
    }

    public partial class VirtualConsole : IDisposable
    {
        /// <inheritdoc />
        public void Dispose()
        {
            _inputStream.Dispose();
            _outputStream.Dispose();
            _errorStream.Dispose();
            _cts.Dispose();
            Input.Dispose();
            Output.Dispose();
            Error.Dispose();
        }
    }
}