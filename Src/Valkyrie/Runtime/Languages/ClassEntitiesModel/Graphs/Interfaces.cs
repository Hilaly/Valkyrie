using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Valkyrie
{
    public interface INode
    {
        IGraph Graph { get; }
        string Uid { get; }

        Rect NodeRect { get; set; }
        Vector2 NodePosition { get; set; }

        Dictionary<string, IValuePort> ValueInPorts { get; }
        Dictionary<string, IValuePort> ValueOutPorts { get; }
        Dictionary<string, IFlowPort> FlowInPorts { get; }
        Dictionary<string, IFlowPort> FlowOutPorts { get; }
        
        IReflectionData GetData();
        void Define(IGraph graph);
    }

    public interface IReflectionData
    {
        string Name { get; }
        string Tooltip { get; }
        Vector2 MinSize { get; }
        bool Deletable { get; }
        bool Movable { get; }

        Type EditorView { get; }
        
        public List<IValuePortAttribute> ValuePorts { get; }
        
        public List<IFlowPortAttribute> FlowPorts { get; }

        INode Create();
    }

    public interface IPort
    {
        string Uid { get; }
        Direction Direction { get; }
        Port.Capacity Capacity { get; }
        Type ValueType { get; }
        string Name { get; }
        Orientation Orientation { get; }
    }

    public interface IValuePort : IPort
    {
        void Definition(INode node, IValuePortAttribute info);
    }
    public interface IFlowPort : IPort
    {}

    public interface IGraph
    {
        string Uid { get; }
        int NodeCount { get; }
        
        IEnumerable<INode> Nodes { get; }
        
        T Add<T>(T node) where T : INode;
        void Remove(INode node);
        void Disconnect(IPort outputPort, IPort inputPort);
        void Connect(IPort outputPort, IPort inputPort);
        void MarkDirty();
    }

    public interface IPortAttribute
    {
        FieldInfo Info { get; }
        string Name { get; }
        Direction Direction { get; }
        Port.Capacity Capacity { get; }
        Orientation Orientation { get; set; }

        void SetInfo(FieldInfo info);
    }

    public interface IValuePortAttribute : IPortAttribute
    {
    }

    public interface IFlowPortAttribute : IPortAttribute
    {
    }

}