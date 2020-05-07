using System.Collections.Generic;

namespace SoupDiscover.Core
{
    internal class RootElement
    {
        private IList<string> _subElements;
        public string Name { get; }
        public RootElement(string name)
        {
            Name = name;
        }
        public IEnumerable<string> SubElement => _subElements;

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
