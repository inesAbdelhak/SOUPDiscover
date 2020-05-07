using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoupDiscover.Core
{
    internal class ConfigFile
    {
        private IList<RootElement> _rootElements;
        private bool _isUpdated;
        public ConfigFile(string path)
        {
            Path = path;
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "");
                _rootElements = new List<RootElement>();
            }
            else
            {
                _rootElements = ReadConfigFile(path).ToList();
            }
        }

        /// <summary>
        /// Add a record to the config file
        /// </summary>
        /// <param name="root">the root element name</param>
        /// <param name="subElement">the sub-element to add</param>
        /// <returns></returns>
        public bool Add(string root, string subElement)
        {
            var rootElement = _rootElements.FirstOrDefault(e => e.Name == root);
            if (rootElement == null)
            {
                rootElement = new RootElement(root);
                _rootElements.Add(rootElement);
            }
            bool result;
            _isUpdated |= result = rootElement.AddSubElement(subElement);
            return result;
        }

        public bool Save()
        {
            if(!_isUpdated)
            {
                return false; // No need to update
            }
            using (var file = new StreamWriter(Path, false))
            {
                foreach (var r in _rootElements)
                {
                    file.WriteLine(r.Name);
                    foreach (var s in r.SubElement)
                    {
                        file.WriteLine($"  {s}");
                    }
                    file.WriteLine();
                }
            }
            _isUpdated = false;
            return true;
        }

        public string Path { get; }

        private static IEnumerable<RootElement> ReadConfigFile(string sshConfigFile)
        {
            RootElement currentRoot = null;
            foreach (var l in File.ReadAllLines(sshConfigFile))
            {
                if (string.IsNullOrWhiteSpace(l))
                {
                    continue; // Empty line
                }
                if (l.StartsWith(' '))
                {
                    if (currentRoot == null)
                    {
                        continue; // a sub-element but no rootElement
                    }
                    currentRoot.AddSubElement(l.Trim());
                }
                yield return currentRoot = new RootElement(l.TrimEnd());
            }
        }
    }
}
