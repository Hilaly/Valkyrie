using System;
using UnityEditor.Experimental.GraphView;

namespace Valkyrie.Model
{
    public interface IPort
    {
        string Uid { get; }
        string Name { get; }
        
        Orientation Orientation { get; set; }
        Port.Capacity Capacity { get; set; }
        Type Type { get; set; }
        Direction Direction { get; }
    }
    
    public interface IInputPort : IPort
    {}
    
    public interface IOutputPort: IPort
    {
    }
}