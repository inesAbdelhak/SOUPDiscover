using System.Collections.Generic;

namespace SoupDiscover.Core
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
        public bool AddSubElement(string subElement)
        {
            if (!_subElements.Contains(subElement))
            {
                _subElements.Add(subElement);
                return true;
            }
            return false;
        }
    }
}
