using UnityEditor.Experimental.GraphView;

namespace Valkyrie.Model
{
    class CemOutputPort<T> : GenericPort<T>, IOutputPort
    {
        public override Direction Direction => Direction.Output;
    }
}