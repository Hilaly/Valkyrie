using Newtonsoft.Json;
using UnityEditor.Experimental.GraphView;
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
        
        [Output("Flow", Capacity = Port.Capacity.Multi), JsonIgnore, DependsOnProperty("Name"), DependsOnProperty("Type")]
        public IFlow FlowOutput { get; }


        [ExportProperty(Name = "Name")] public string PropertyName
        {
            get => Output.Name;
            set => Output.Name = value;
        }
    }
}