using System.IO;

namespace CliFx.Infrastructure
{
    class FileSystem : IFileSystem
    {
        public void Copy(string sourceFileName, string destFileName)
        {
            File.Copy(sourceFileName, destFileName);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content);
        }
    }
}
