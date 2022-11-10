using UnityEditor.Experimental.GraphView;

namespace Valkyrie.Model
{
    class CemInputPort<T> : GenericPort<T>, IInputPort
    {
        public override Direction Direction => Direction.Input;
    }
}