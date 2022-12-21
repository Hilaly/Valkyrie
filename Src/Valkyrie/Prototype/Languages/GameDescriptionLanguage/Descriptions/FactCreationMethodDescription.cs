using System.Collections.Generic;

namespace Valkyrie.Language.Description
{
    public class FactCreationMethodDescription
    {
        public ComponentDescription Component;

        public string EntityIdExpr;
        public List<string> Arguments = new List<string>();

        public FactCreationMethodDescription(ComponentDescription component)
        {
            Component = component;
        }
    }
}