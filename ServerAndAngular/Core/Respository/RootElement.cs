using System.Collections.Generic;
using System.Linq;

namespace SoupDiscover.Core.Respository
{
    /// <summary>
    /// A root Element in ssh config file.
    /// Ex :
    /// host * -> RootElement
    ///   hostname toto -> Sub-Element
    /// </summary>
    internal class RootElement
    {
        private IList<string> _subElements = new List<string>();

        /// <summary>
        /// The name of the RootElemet. Ex : "Host *"
        /// </summary>
        public string Name { get; }

        internal RootElement(string name)
        {
            Name = name;
        }

        /// <summary>
        /// All subElement of the RootElement
        /// </summary>
        public IEnumerable<string> SubElement => _subElements;

        /// <summary>
        /// Add a subElement in the rootElement
        /// </summary>
        /// <param name="subElement">The text of the subElement</param>
        /// <returns></returns>
        public bool AddSubElement(string subElementKey, string subElementValue)
        {
            var currentSubElement = _subElements.FirstOrDefault(e => e.StartsWith($"{subElementKey} "));
            if (currentSubElement == $"{subElementKey} {subElementValue}")
            {
                return false;
            }
            if (currentSubElement != null)
            {
                // remove the current subElement
                _subElements.Remove(currentSubElement);
            }
            // Add the new SubElement
            _subElements.Add($"{subElementKey} {subElementValue}");
            return true;
        }

        public bool AddSubElement(string subElement)
        {
            var subElementSplit = subElement.Split(' ');
            if (subElementSplit.Length != 2)
            {
                return false; // The subElement contains only a key. It will not be added
            }
            return AddSubElement(subElementSplit[0], subElementSplit[1]);
        }
    }
}
