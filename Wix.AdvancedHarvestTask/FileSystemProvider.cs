using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wix.AdvancedHarvestTask
{
    internal class FileSystemProvider : IFileSystem
    {
        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public IEnumerable<string> EnumerateFiles(string path, string pattern)
        {
            return Directory.EnumerateFiles(path, pattern, SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<string> EnumerateFolders(string path)
        {
            return Directory.EnumerateDirectories(path, "*.*", SearchOption.TopDirectoryOnly);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public void RemoveReadonly(string path)
        {
            File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.Normal);
        }

        public void Save(XmlDocument doc, string path)
        {
            doc.Save(path);
        }
    }
}
