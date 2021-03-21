using System.IO;
using System.Text;

namespace CliFx.Infrastructure
{
    /// <summary>
    /// Implements a <see cref="TextReader"/> for reading characters from a console stream.
    /// </summary>
    public partial class ConsoleReader : StreamReader
    {
        /// <summary>
        /// Console that owns this stream.
        /// </summary>
        public IConsole Console { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ConsoleReader"/>.
        /// </summary>
        public ConsoleReader(IConsole console, Stream stream, Encoding encoding)
            : base(stream, encoding, false)
        {
            Console = console;
        }

        /// <summary>
        /// Initializes an instance of <see cref="ConsoleReader"/>.
        /// </summary>
        public ConsoleReader(IConsole console, Stream stream)
            : this(console, stream, System.Console.InputEncoding)
        {
        }
    }

    public partial class ConsoleReader
    {
        internal static ConsoleReader Create(IConsole console, Stream? stream) =>
            new(console, stream is not null ? Stream.Synchronized(stream) : Stream.Null);
    }
}