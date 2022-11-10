using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    class ListPropertyNode : CemNode, IPropertyNode
    {
        public class Factory : SimpleGenericFactory<ListPropertyNode>
        {
            public Factory() : base("List", "Properties") { }
        }

        [Output("Output"), JsonIgnore, DependsOnProperty("Name")]
        [field: JsonProperty]
        public PropertyDefine Output { get; } = new();
        
        [Output("Flow", Capacity = Port.Capacity.Multi), JsonIgnore, DependsOnProperty("Name"), DependsOnProperty("Type")]
        public IFlow FlowOutput { get; }


        [Input("Input"), JsonIgnore]
        public PropertyDefine Input
        {
            set => Output.Type = $"{typeof(List<>).Namespace}.List<{value.Type}>";
        }

        [ExportProperty(Name = "Name")]
        public string PropertyName
        {
            get => Output.Name;
            set => Output.Name = value;
        }
    }
}