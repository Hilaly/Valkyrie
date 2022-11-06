using System.Collections.Generic;

namespace Valkyrie.Model
{
    public class CemNodeChangedEvent
    {
        public bool renamed;
        public List<IPort> portAdded;
        public List<IPort> portRemoved;
        public List<IPort> portRenamed;
        public List<IPort> portValueChanged;

        public static CemNodeChangedEvent AddPort(IPort port) =>
            new()
            {
                portAdded = new List<IPort>() { port }
            };

        public static CemNodeChangedEvent RemovePort(IPort port) =>
            new()
            {
                portRemoved = new List<IPort>() { port }
            };
        
        public static CemNodeChangedEvent PortRenamed(IPort port) =>
            new()
            {
                portRenamed = new List<IPort>() { port }
            };
        
        public static CemNodeChangedEvent PortValueChanged(IPort port) =>
            new()
            {
                portValueChanged = new List<IPort>() { port }
            };

        public static CemNodeChangedEvent Renamed() =>
            new()
            {
                renamed = true
            };
    }
}