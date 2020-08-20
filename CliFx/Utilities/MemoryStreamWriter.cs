using System.IO;
using System.Text;

namespace CliFx.Utilities
{
    /// <summary>
    /// Implementation of <see cref="StreamWriter"/> with a <see cref="MemoryStream"/> as a backing store.
    /// </summary>
    public class MemoryStreamWriter : StreamWriter
    {
        private new MemoryStream BaseStream => (MemoryStream) base.BaseStream;

        /// <summary>
        /// Initializes an instance of <see cref="MemoryStreamWriter"/>.
        /// </summary>
        public MemoryStreamWriter(Encoding encoding)
            : base(new MemoryStream(), encoding)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="MemoryStreamWriter"/>.
        /// </summary>
        public MemoryStreamWriter()
            : base(new MemoryStream())
        {
        }

        /// <summary>
        /// Gets the bytes written to the underlying stream.
        /// </summary>
        public byte[] GetBytes()
        {
            Flush();
            return BaseStream.ToArray();
        }

        /// <summary>
        /// Gets the string written to the underlying stream.
        /// </summary>
        public string GetString() => Encoding.GetString(GetBytes());
    }
}