using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Valkyrie.XPath
{
    public class XPath
    {
        private readonly string _expression;
        private readonly IPathSelector _selector;

        public XPath(string expression)
        {
            _expression = expression;
            _selector = XPathBuilder.BuildSelector(expression);
        }

        public override string ToString() => $"XPath:{_expression}";

        public IEnumerable<XPathElement> SelectNodes(GameObject startNode) =>
            _selector.SelectNodes(Enumerable.Repeat(new XPathElement(startNode), 1));

        public XPathElement SelectSingleNode(GameObject startNode) => SelectNodes(startNode).FirstOrDefault();
    }
}