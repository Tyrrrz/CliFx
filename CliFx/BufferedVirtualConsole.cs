using System;
using System.IO;
using System.Threading;

namespace CliFx
{
    public class BufferedVirtualConsole : VirtualConsole, IDisposable
    {
        private MemoryStream InputStream => (MemoryStream) Input.BaseStream;

        private MemoryStream OutputStream => (MemoryStream) Output.BaseStream;

        private MemoryStream ErrorStream => (MemoryStream) Error.BaseStream;

        /// <summary>
        /// Initializes an instance of <see cref="BufferedVirtualConsole"/>.
        /// </summary>
        public BufferedVirtualConsole(CancellationToken cancellationToken = default)
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
            InputStream.Write(data);
            InputStream.Flush();
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