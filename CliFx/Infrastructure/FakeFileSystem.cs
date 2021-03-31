using System;
using System.Collections.Generic;
using System.Text;

namespace CliFx.Infrastructure
{

#pragma warning disable 1591
    /// <summary>
    /// A mock for IFileSystem
    /// </summary>
    public class FakeFileSystem : IFileSystem
    {
        public Dictionary<string, string> Files = new Dictionary<string, string>();

        public IEnumerable<string> FilePaths => Files.Keys;

        public void Copy(string sourceFileName, string destFileName)
        {
            if( Files.ContainsKey(sourceFileName))
            {
                Files[destFileName] = Files[sourceFileName];
            }
        }

        public bool Exists(string path)
        {
            return Files.ContainsKey(path);
        }

        public bool TryReadText(string path, out string content)
        {
            if( Files.ContainsKey(path))
            {
                content = Files[path];
                return true;
            }
            content = "";
            return false;
        }

        public void WriteText(string path, string content)
        {
            Files[path] = content;
        }
    }

#pragma warning restore 1591
}
