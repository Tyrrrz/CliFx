using System.IO;

namespace CliFx.Infrastructure
{
    internal class FileSystem : IFileSystem
    {
        public void Copy(string sourceFileName, string destFileName)
        {
            File.Copy(sourceFileName, destFileName);
        }

        public bool Exists(string path)
        {
            return new FileInfo(path).Exists;
        }

        public bool TryReadText(string path, out string text)
        {
            if (!File.Exists(path))
            {
                text = "";
                return false;
            }
            text = File.ReadAllText(path);
            return true;
        }

        public void WriteText(string path, string content)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllText(path, content);
        }
    }
}
