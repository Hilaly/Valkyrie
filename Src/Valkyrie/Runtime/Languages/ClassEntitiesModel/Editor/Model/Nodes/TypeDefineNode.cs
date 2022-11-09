using UnityEditor.Experimental.GraphView;
using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    abstract class TypeDefineNode<T> :
        CemGraph, IRenamable
        where T : TypeDefineNode<T>
    {
        public override void OnCreate()
        {
            CreateInputPort<T>("Parents").Capacity = Port.Capacity.Multi;
            CreateOutputPort<T>("Self").Capacity = Port.Capacity.Multi;

            CreateInputPort<PropertyDefine>("Properties").Capacity = Port.Capacity.Multi;
        }
    }
}