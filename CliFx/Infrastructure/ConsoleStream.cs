using System;
using System.IO;

namespace CliFx.Infrastructure
{
    internal static class ConsoleStream
    {
        public static StreamReader WrapInput(Stream? stream) =>
            stream is not null
                ? new StreamReader(Stream.Synchronized(stream), Console.InputEncoding, false)
                : StreamReader.Null;

        public static StreamWriter WrapOutput(Stream? stream) =>
            stream is not null
                ? new StreamWriter(Stream.Synchronized(stream), Console.OutputEncoding) {AutoFlush = true}
                : StreamWriter.Null;
    }
}