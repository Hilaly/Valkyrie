using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Model
{
    class TestGraph : CemGraph
    {
        public override IEnumerable<INodeFactory> GetFactories()
        {
            return Enumerable.Empty<INodeFactory>();
        }
    }
}