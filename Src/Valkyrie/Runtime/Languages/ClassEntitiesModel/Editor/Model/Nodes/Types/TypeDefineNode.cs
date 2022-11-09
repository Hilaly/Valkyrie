using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Scripting;
using Valkyrie.Utils;

namespace Valkyrie.Model.Nodes
{
    interface ITypeDefine
    {
        string Name { get; }
    }
    
    [Preserve]
    abstract class TypeDefineNode<T> :
        CemGraph, IRenamable, ITypeDefine
        where T : TypeDefineNode<T>
    {
        public override void OnCreate()
        {
            base.OnCreate();
            CreateInputPort<T>("Parents").Capacity = Port.Capacity.Multi;
            CreateOutputPort<T>("Self").Capacity = Port.Capacity.Multi;
        }

        protected override void EnsureNodesExist()
        {
            base.EnsureNodesExist();

            if (Nodes.FirstOrDefault(x => x is PropertiesEndPointNode) == null)
                Create(new PropertiesEndPointNode.Factory());
        }

        public override void PrepareForDrawing()
        {
            base.PrepareForDrawing();

            foreach (var endPointNode in Nodes.OfType<PropertiesEndPointNode>())
            {
                foreach (var node in 
                    this.GetPreviousNodes(endPointNode.GetPort("Input")))
                {
                    var propertyNode = (IPropertyNode)node;
                    var portName = propertyNode.Output.Name;
                    var portType = Graph.GetTypedValue(propertyNode.Output.Type);
                    if (portType == null)
                    {
                        Debug.LogWarning($"[CEM]: Property {portName} can not be create4d");
                        continue;
                    }
                    
                    var existPort = GetPort(portName);
                    if (existPort == null) 
                        CreatePort(portName, portType, Direction.Output, Port.Capacity.Multi);
                    else if (existPort.Type != portType)
                    {
                        RemovePort(existPort);
                        CreatePort(portName, portType, Direction.Output, Port.Capacity.Multi);
                    }
                }
            }
        }
    }
}