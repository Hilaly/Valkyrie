using System.Collections.Generic;

namespace Valkyrie.XPath
{
    public interface IPathSelector
    {
        IEnumerable<XPathElement> SelectNodes(IEnumerable<XPathElement> nodes);
    }
}