using System.Collections.Generic;

namespace Valkyrie.Model
{
    public class CemNodeChangedEvent
    {
        public List<IPort> portAdded;
        public List<IPort> portRemoved;

        public static CemNodeChangedEvent AddPort(IPort port) =>
            new()
            {
                portAdded = new List<IPort>() { port }
            };

        public static CemNodeChangedEvent RemovePort(IPort port) =>
            new()
            {
                portAdded = new List<IPort>() { port }
            };
    }
}