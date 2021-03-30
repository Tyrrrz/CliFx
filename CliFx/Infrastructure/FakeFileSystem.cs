using System;
using System.Collections.Generic;
using System.Text;

namespace CliFx.Infrastructure
{
    public class FakeSystem : IFileSystem
    {
        public Dictionary<string, string> Files => new Dictionary<string, string>();

        public void Copy(string sourceFileName, string destFileName)
        {
            Files[destFileName] = Files[sourceFileName];
        }

        public bool Exists(string path)
        {
            return Files.ContainsKey(path);
        }

        public string ReadAllText(string path)
        {
            return Files[path];
        }

        public void WriteAllText(string path, string content)
        {
            Files[path] = content;
        }
    }


}
