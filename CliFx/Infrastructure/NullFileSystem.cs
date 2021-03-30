using System;
using System.Collections.Generic;
using System.Text;

namespace CliFx.Infrastructure
{
    public class NullFileSystem : IFileSystem
    {
        public void Copy(string sourceFileName, string destFileName)
        {
        }

        public bool Exists(string path)
        {
            return false;
        }

        public string ReadAllText(string path)
        {
            return "";
        }

        public void WriteAllText(string path, string content)
        {
        }
    }
}
