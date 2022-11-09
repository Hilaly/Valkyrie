using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Scripting;
using Valkyrie.Utils;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    class TypeReferenceNode<T> : CemNode
        where T : TypeDefineNode<T>
    {
        public class Factory : SimpleGenericFactory<TypeReferenceNode<T>>
        {
            public Factory() : base($"Ref {typeof(T).Name}", "Types")
            {
            }
        }
        
        public override void OnCreate()
        {
            base.OnCreate();
        }

        [ExportProperty(Name = nameof(Typename))]
        public string Typename
        {
            get => Output?.Name ?? string.Empty;
            set => Output = Graph.GetGraphRoot().GetAllSubNodes<T>().FirstOrDefault(x => x.Name == value);
        }
        
        [Output("Output", Capacity = Port.Capacity.Multi), DependsOnProperty(nameof(Typename))]
        public T Output { get; set; }
    }
}