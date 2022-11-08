using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    abstract class GenericPropertyNode<T> : CemNode, IPropertyNode
    {
        [Output("Output"), JsonIgnore, DependsOnProperty("Name")]
        [field: JsonProperty]
        public PropertyDefine Output { get; } = new()
        {
            Type = typeof(T).FullName
        };

        [ExportProperty(Name = "Name")] public string PropertyName
        {
            get => Output.Name;
            set => Output.Name = value;
        }
    }
}