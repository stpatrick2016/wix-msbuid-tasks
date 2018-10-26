using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wix.AdvancedHarvestTask
{
    internal interface IFileSystem
    {
        bool FileExists(string path);
        bool DirectoryExists(string path);
        void RemoveReadonly(string path);
        void CreateDirectory(string path);
        IEnumerable<string> EnumerateFiles(string path, string pattern);
        IEnumerable<string> EnumerateFolders(string path);
        void Save(XmlDocument doc, string path);
    }
}
