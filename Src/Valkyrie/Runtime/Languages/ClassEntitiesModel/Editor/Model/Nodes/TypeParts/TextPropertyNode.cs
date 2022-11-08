using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    class CustomPropertyNode : CemNode, IPropertyNode
    {
        [Output("Output"), JsonIgnore, DependsOnProperty("Name"), DependsOnProperty("Type")]
        [field: JsonProperty]
        public PropertyDefine Output { get; } = new();

        [ExportProperty(Name = "Name")]
        public string PropertyName
        {
            get => Output.Name;
            set => Output.Name = value;
        }

        [ExportProperty(Name = "Type")]
        public string PropertyType
        {
            get => Output.Type;
            set => Output.Type = value;
        }
        
        
        public class Factory : SimpleGenericFactory<CustomPropertyNode>
        {
            public Factory() : base("Custom property", "Properties") { }
        }
    }
}