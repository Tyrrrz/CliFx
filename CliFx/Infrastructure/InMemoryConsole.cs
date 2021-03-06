using System;
using System.IO;
using System.Threading;

namespace CliFx.Infrastructure
{
    /// <summary>
    /// Implementation of <see cref="IConsole"/> that uses in-memory standard input, output, and error streams
    /// instead of the ones exposed by the system console.
    /// This implementation is designed for use in tests.
    /// </summary>
    public class InMemoryConsole : RedirectedConsole, IDisposable
    {
        private MemoryStream InputStream => (MemoryStream) Input.BaseStream;

        private MemoryStream OutputStream => (MemoryStream) Output.BaseStream;

        private MemoryStream ErrorStream => (MemoryStream) Error.BaseStream;

        /// <summary>
        /// Initializes an instance of <see cref="InMemoryConsole"/>.
        /// </summary>
        public InMemoryConsole(CancellationToken cancellationToken = default)
            : base(
                new MemoryStream(), true,
                new MemoryStream(), true,
                new MemoryStream(), true,
                cancellationToken)
        {
        }

        /// <summary>
        /// Writes data to the input stream.
        /// </summary>
        public void WriteInput(byte[] data)
        {
            // TODO: is this safe?
            var lastPosition = InputStream.Position;

            InputStream.Write(data);
            InputStream.Flush();

            InputStream.Position = lastPosition;
        }

        /// <summary>
        /// Writes data to the input stream.
        /// </summary>
        public void WriteInput(string data) => WriteInput(Input.CurrentEncoding.GetBytes(data));

        /// <summary>
        /// Reads the data written to the output stream.
        /// </summary>
        public byte[] ReadOutputBytes()
        {
            OutputStream.Flush();
            return OutputStream.ToArray();
        }

        /// <summary>
        /// Reads the data written to the output stream.
        /// </summary>
        public string ReadOutputString() => Output.Encoding.GetString(ReadOutputBytes());

        /// <summary>
        /// Reads the data written to the error stream.
        /// </summary>
        public byte[] ReadErrorBytes()
        {
            ErrorStream.Flush();
            return ErrorStream.ToArray();
        }

        /// <summary>
        /// Reads the data written to the error stream.
        /// </summary>
        public string ReadErrorString() => Error.Encoding.GetString(ReadErrorBytes());

        /// <inheritdoc />
        public void Dispose()
        {
            InputStream.Dispose();
            OutputStream.Dispose();
            ErrorStream.Dispose();
        }
    }
}