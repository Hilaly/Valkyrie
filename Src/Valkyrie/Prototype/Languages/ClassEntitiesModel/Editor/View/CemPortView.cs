using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Valkyrie.View
{
    class CemPortView : Port, IPortView
    {
        protected internal CemPortView(Orientation portOrientation, Direction portDirection, Capacity portCapacity,
            Type type, IEdgeConnectorListener listener) : base(portOrientation, portDirection, portCapacity, type)
        {
            m_EdgeConnector = new EdgeConnector<Edge>(listener);
            this.AddManipulator(m_EdgeConnector);
        }
    }
}