using System.Collections.Generic;

namespace Valkyrie.Language.Description
{
    public class FactsFilterMethodDescription
    {
        public string Name;
        public List<ComponentDescription> Components = new List<ComponentDescription>();
        public List<string> Operators = new List<string>();
        public bool IsReference;
    }
}