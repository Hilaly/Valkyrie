using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Valkyrie
{
    public class DummyReflection : IReflectionData
    {
        public string Name => "Dummy";
        public string Tooltip => "Tooltip dummy";
        public Vector2 MinSize => new Vector2(200, 100);
        public bool Deletable => true;
        public bool Movable => true;
        public Type EditorView { get; }

        public INode Create()
        {
            return new DummyNode();
        }
    }

    public class DummyNode : INode
    {
        private Rect _nodeRect;

        public IGraph Graph { get; set; }

        public string Uid { get; } = Guid.NewGuid().ToString();

        public Rect NodeRect
        {
            get => _nodeRect;
            set => _nodeRect = value;
        }

        public Vector2 NodePosition
        {
            get => _nodeRect.position;
            set => _nodeRect.position = value;
        }

        public Dictionary<string, IPort> ValueInPorts { get; protected set; } = new();
        public Dictionary<string, IPort> ValueOutPorts { get; protected set; } = new();

        public Dictionary<string, IPort> FlowInPorts { get; protected set; } = new();
        public Dictionary<string, IPort> FlowOutPorts { get; protected set; } = new();
        
        public IReflectionData GetData()
        {
            return new DummyReflection();
        }

        public void Define(IGraph graph)
        {
            Graph = graph;
        }

        public DummyNode()
        {
            FlowInPorts.Add("Input", new CustomFlowPort()
            {
                Capacity = Port.Capacity.Multi,
                Direction = Direction.Input,
                Name = "input.test",
                Orientation = Orientation.Vertical,
                ValueType = typeof(IInheritance)
            });
            FlowOutPorts.Add("Output", new CustomFlowPort()
            {
                Capacity = Port.Capacity.Multi,
                Direction = Direction.Output,
                Name = "output.test",
                Orientation = Orientation.Vertical,
                ValueType = typeof(IInheritance)
            });
        }
    }

}