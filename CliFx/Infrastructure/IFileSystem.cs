using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CliFx.Infrastructure
{
    /// <summary>
    /// Abstraction for the file system
    /// </summary>
    public interface IFileSystem
    {
        /// <summary>
        /// Determines whether the specified file exists. 
        /// Doesn't work in Linux Subsystem for Windows unless application if the application resides on a windows mount (eg /mnt/c/...)
        /// </summary>
        bool Exists(string filePath);

        /// <summary>
        /// Attempts to open a text file, reads all the text in the file, and then closes the file.
        /// </summary>
        bool TryReadText(string filePath, out string content);

        /// <summary>
        /// Creates a new file (and any intermediate directories required), writes the specified string to the file, 
        /// and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        void WriteText(string filePath, string content);

        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is
        /// not allowed.
        /// </summary>
        void Copy(string path, string backupPath);
    }
}
