using System.IO;
using System.Text;

namespace CliFx.Infrastructure
{
    /// <summary>
    /// Implements a <see cref="TextWriter"/> for writing characters to a console stream.
    /// </summary>
    public partial class ConsoleWriter : StreamWriter
    {
        /// <summary>
        /// Console that owns this stream.
        /// </summary>
        public IConsole Console { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ConsoleWriter"/>.
        /// </summary>
        public ConsoleWriter(IConsole console, Stream stream, Encoding encoding)
            : base(stream, encoding)
        {
            Console = console;
        }

        /// <summary>
        /// Initializes an instance of <see cref="ConsoleWriter"/>.
        /// </summary>
        public ConsoleWriter(IConsole console, Stream stream)
            : this(console, stream, System.Console.OutputEncoding)
        {
        }
    }

    public partial class ConsoleWriter
    {
        internal static ConsoleWriter Create(IConsole console, Stream? stream) =>
            new(console, stream is not null ? Stream.Synchronized(stream) : Stream.Null) {AutoFlush = true};
    }
}