using System;
using System.Collections.Generic;
using System.Text;

namespace CliFx.Infrastructure
{
#pragma warning disable 1591
    /// <summary>
    /// Null Pattern implementation of IFileSystem. Does nothing, returns false.
    /// </summary>
    public class NullFileSystem : IFileSystem
    {
        public void Copy(string sourceFileName, string destFileName)
        {
        }

        public bool Exists(string path)
        {
            return false;
        }

        public bool TryReadText(string path, out string content)
        {
            content = "";
            return false;
        }

        public void WriteText(string path, string content)
        {
        }
    }
#pragma warning restore 1591
}
