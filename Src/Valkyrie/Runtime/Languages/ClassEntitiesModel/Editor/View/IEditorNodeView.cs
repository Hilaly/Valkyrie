using UnityEditor.Experimental.GraphView;

namespace Valkyrie.View
{
    public interface IEditorNodeView
    {
        IEdgeConnectorListener EdgeListener { get; set; }
    }
}