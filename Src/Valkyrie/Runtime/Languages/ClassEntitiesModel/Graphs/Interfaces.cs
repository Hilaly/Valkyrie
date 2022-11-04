using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Valkyrie
{
    public interface INodeContent
    {
        void FillBody(VisualElement container);
    }
    
    public interface INode : INodeSection
    {
        IGraph Graph { get; }
        string Uid { get; }

        Rect NodeRect { get; set; }
        Vector2 NodePosition { get; set; }
        
        IReadOnlyList<IFlowPort> FlowInPorts { get; }
        IReadOnlyList<IFlowPort> FlowOutPorts { get; }
        
        INodeFactory GetData();
        
        void Define(IGraph graph);
        void Define();
    }

    public interface INodeSection
    {
        public string Name { get; set; }

        IReadOnlyList<IValuePort> ValueInPorts { get; }
        IReadOnlyList<IValuePort> ValueOutPorts { get; }
    }

    public interface INodeFactory
    {
        public HashSet<string> Tags { get; }
        public string Path { get; }
        string Name { get; }
        Vector2 MinSize { get; }
        string Tooltip { get; }
        bool Deletable { get; }
        bool Movable { get; }

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