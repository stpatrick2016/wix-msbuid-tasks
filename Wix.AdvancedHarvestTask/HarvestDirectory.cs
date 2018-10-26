using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Wix.AdvancedHarvestTask
{
    public class HarvestDirectory : Task
    {
        private const string NS_WIX = "http://schemas.microsoft.com/wix/2006/wi";
        private readonly IFileSystem _fileSystem;
        private static Regex _componentIdRegex = new Regex("^[^a-zA-Z_]|[^a-zA-Z0-9_\\.]");
        private Dictionary<string, int> _componentIds = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, int> _dirIds = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        public HarvestDirectory()
            : this(new FileSystemProvider())
        {

        }

        internal HarvestDirectory(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public override bool Execute()
        {
            string[] includes = (string.IsNullOrEmpty(IncludeMask) ? "*.*" : IncludeMask).Split(';');
            string[] excludes = string.IsNullOrEmpty(ExcludeMask) ? new string[0] : ExcludeMask.Split(';');

            var doc = new XmlDocument();
            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace(string.Empty, NS_WIX);
            var root = doc.CreateElement(null, "Wix", NS_WIX);
            doc.AppendChild(root);

            //create fragment for directories
            var fragment = doc.CreateElement(null, "Fragment", NS_WIX);
            root.AppendChild(fragment);

            var dir = doc.CreateElement(null, "DirectoryRef", NS_WIX);
            dir.SetAttribute("Id", DirectoryRefId);
            fragment.AppendChild(dir);

            //and fragment for component group
            fragment = doc.CreateElement(null, "Fragment", NS_WIX);
            root.AppendChild(fragment);
            var group = doc.CreateElement(null, "ComponentGroup", NS_WIX);
            group.SetAttribute("Id", ComponentGroupName);
            fragment.AppendChild(group);

            AddDirectory(
                SourceFolder,
                includes,
                excludes,
                dir,
                group,
                n => doc.CreateElement(null, n, NS_WIX));

            if(_fileSystem.FileExists(OutputFile))
            {
                //remove readonly hidden and whatever attributes the file had
                _fileSystem.RemoveReadonly(OutputFile);
            }
            else if(!_fileSystem.DirectoryExists(Path.GetDirectoryName(Path.GetFullPath(OutputFile))))
            {
                //otherwise check that parent directory exists and create if not
                _fileSystem.CreateDirectory(Path.GetDirectoryName(OutputFile));
            }

            _fileSystem.Save(doc, OutputFile);
            return true;
        }

        private bool AddDirectory(string path, string[] includes, string[] excludes, XmlNode dirNode, XmlNode groupNode, Func<string, XmlElement> createNode)
        {
            HashSet<string> excludedFiles = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            List<string> files = new List<string>();

            //find all files that should be excluded
            foreach (var pattern in excludes)
            {
                foreach(var f in _fileSystem.EnumerateFiles(path, pattern))
                {
                    excludedFiles.Add(f);
                }
            }

            foreach (var pattern in includes)
            {
                foreach (var f in _fileSystem.EnumerateFiles(path, pattern))
                {
                    if (!excludedFiles.Contains(f))
                    {
                        files.Add(f);
                    }
                }
            }
            
            if(!files.Any() && !IncludeEmptyDirectories)
            {
                //folder is empty and we were asked not to include empty folders
                return false;
            }

            //ID may only include a-z, A-Z 0-9, underscore or period and must start with eirther underscore or letter
            foreach (var file in files)
            {
                var compId = GenerateComponentId(file);

                //add the component under directory
                var comp = createNode("Component");
                comp.SetAttribute("Id", compId);
                comp.SetAttribute("Guid", "*");
                dirNode.AppendChild(comp);

                //add file node
                var fnode = createNode("File");
                fnode.SetAttribute("Id", compId);
                fnode.SetAttribute("Name", Path.GetFileName(file));
                fnode.SetAttribute("Source", file);
                fnode.SetAttribute("KeyPath", "yes");
                if (!string.IsNullOrEmpty(DiskId))
                {
                    fnode.SetAttribute("DiskId", DiskId);
                }
                if(!string.IsNullOrEmpty(DefaultFileVersion))
                {
                    fnode.SetAttribute("DefaultFileVersion", DefaultFileVersion);
                }
                comp.AppendChild(fnode);

                //add component to component group
                comp = createNode("ComponentRef");
                comp.SetAttribute("Id", compId);
                groupNode.AppendChild(comp);
            }

            foreach (var d in _fileSystem.EnumerateFolders(path))
            {
                var node = createNode("Directory");
                if(AddDirectory(d, includes, excludes, node, groupNode, createNode))
                {
                    node.SetAttribute("Id", GenerateDirectoryId(d));
                    node.SetAttribute("Name", Path.GetFileName(d));
                    dirNode.AppendChild(node);
                }
            }

            return true;
        }

        private string GenerateComponentId(string filepath)
        {
            var compId = _componentIdRegex.Replace(Path.GetFileName(filepath), "_");

            //in case same file appears in subfolders - append counter to it
            if (_componentIds.ContainsKey(compId))
            {
                _componentIds[compId] += 1;
                compId = $"{compId}_{_componentIds[compId]}";
            }
            else
            {
                _componentIds.Add(compId, 1);
            }

            if (!string.IsNullOrEmpty(ComponentPrefix))
            {
                compId = ComponentPrefix + "_" + compId;
            }

            return compId;
        }


        private string GenerateDirectoryId(string dirPath)
        {
            var dirId = _componentIdRegex.Replace(Path.GetFileName(dirPath), "_");

            //in case same file appears in subfolders - append counter to it
            if (_dirIds.ContainsKey(dirId))
            {
                _dirIds[dirId] += 1;
                dirId = $"{dirId}_{_dirIds[dirId]}";
            }
            else
            {
                _dirIds.Add(dirId, 1);
            }

            return dirId;
        }

        #region Task Properties
        [Required]
        public string SourceFolder { get; set; }

        [Required]
        public string DirectoryRefId { get; set; }

        [Required]
        public string OutputFile { get; set; }

        public string IncludeMask { get; set; }

        public string ExcludeMask { get; set; }

        public string ComponentPrefix { get; set; }

        [Required]
        public string ComponentGroupName { get; set; }

        public string DiskId { get; set; }

        public string DefaultFileVersion { get; set; }

        public bool IncludeEmptyDirectories { get; set; }
        #endregion
    }
}
