using System.Collections.Generic;

namespace Valkyrie.Language.Description
{
    public class ComponentDescription
    {
        public string Name;
        public readonly List<FieldDescription> Fields = new List<FieldDescription>();

        public string GetTypeName() => $"{Name.Replace("Component", string.Empty)}Component";
    }
}