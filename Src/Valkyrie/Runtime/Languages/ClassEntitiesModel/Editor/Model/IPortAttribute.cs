using UnityEditor.Experimental.GraphView;

namespace Valkyrie.Model
{
    public interface IPortAttribute
    {
        string Name { get; }
        Direction Direction { get; }
    }
}