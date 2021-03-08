﻿using System.IO;

namespace CliFx.Infrastructure
{
    /// <summary>
    /// Implementation of <see cref="IConsole"/> that uses fake standard input, output, and error streams
    /// backed by in-memory stores.
    /// </summary>
    /// <remarks>
    /// This implementation is designed for usage in tests.
    /// </remarks>
    public class FakeInMemoryConsole : FakeConsole
    {
        private readonly MemoryStream _input;
        private readonly MemoryStream _output;
        private readonly MemoryStream _error;

        private FakeInMemoryConsole(MemoryStream input, MemoryStream output, MemoryStream error)
            : base(input, output, error)
        {
            _input = input;
            _output = output;
            _error = error;
        }

        /// <summary>
        /// Initializes an instance of <see cref="FakeInMemoryConsole"/>.
        /// </summary>
        public FakeInMemoryConsole()
            : this(new MemoryStream(), new MemoryStream(), new MemoryStream())
        {
        }

        /// <summary>
        /// Writes data to the input stream.
        /// </summary>
        public void WriteInput(byte[] data)
        {
            // TODO: is this safe?
            var lastPosition = _input.Position;

            _input.Write(data);
            _input.Flush();

            _input.Position = lastPosition;
        }

        /// <summary>
        /// Writes data to the input stream.
        /// </summary>
        public void WriteInput(string data) => WriteInput(
            Input.CurrentEncoding.GetBytes(data)
        );

        /// <summary>
        /// Reads the data written to the output stream.
        /// </summary>
        public byte[] ReadOutputBytes()
        {
            _output.Flush();
            return _output.ToArray();
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
            _error.Flush();
            return _error.ToArray();
        }

        /// <summary>
        /// Reads the data written to the error stream.
        /// </summary>
        public string ReadErrorString() => Error.Encoding.GetString(ReadErrorBytes());

        /// <inheritdoc />
        public override void Dispose()
        {
            _input.Dispose();
            _output.Dispose();
            _error.Dispose();

            base.Dispose();
        }
    }
}