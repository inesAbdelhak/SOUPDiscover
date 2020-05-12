using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoupDiscover.Core
{
    /// <summary>
    /// To read create or modify a ssh config file. ~/.ssh/config
    /// </summary>
    public class SshConfigFile
    {
        /// <summary>
        /// All root element of the ssh config file
        /// </summary>
        private IList<RootElement> _rootElements;
        
        private bool _isUpdated;
        
        public SshConfigFile(string path)
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
        /// <returns>true : The key is added, false : the element already exists</returns>
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

        /// <summary>
        /// Save the current state to the file
        /// </summary>
        /// <returns>true: An update is pending ad saved. False : no pending modification.</returns>
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
            if(Environment.OSVersion.Platform == PlatformID.Unix)
            {
                // Update permission to the file
            }
            _isUpdated = false;
            return true;
        }

        /// <summary>
        /// The path to the config file
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// To read all rootElement of the ssh config file
        /// </summary>
        /// <param name="sshConfigFile">the config file path</param>
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
                    continue;
                }
                yield return currentRoot = new RootElement(l.TrimEnd());
            }
        }
    }
}
