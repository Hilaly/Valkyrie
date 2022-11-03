using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valkyrie;

namespace Valkyrie
{
    public class Feature : MainFeatureData, IGraph
    {
        private readonly List<BaseType> _types = new();

        public T Get<T>(string typeName) where T : BaseType => (T)_types.Find(x => x is T && x.Name == typeName);

        private T GetOrCreate<T>(string typeName) where T : BaseType, new()
        {
            var r = Get<T>(typeName);
            if (r == null)
                _types.Add(r = new T { Name = typeName });
            return r;
        }

        public IReadOnlyList<T> Get<T>() where T : BaseType => _types.OfType<T>().ToList();

        public EntityType CreateEntity(string typeName) => GetOrCreate<EntityType>(typeName);
        public ConfigType CreateConfig(string typeName) => GetOrCreate<ConfigType>(typeName);
        public ItemType CreateItem(string typeName) => GetOrCreate<ItemType>(typeName);

        internal void Push(BaseType inst) => _types.Add(inst);

        #region IGraph

        public int NodeCount => Nodes.Count();

        public IEnumerable<INode> Nodes
        {
            get
            {
                return Get<BaseType>().Select(x => new TypeDefineNode(x));
            }
        }

        public void Add(INode node)
        {
            throw new System.NotImplementedException();
        }

        public void Remove(INode node)
        {
            throw new System.NotImplementedException();
        }

        public void Disconnect(IPort outputPort, IPort inputPort)
        {
            throw new System.NotImplementedException();
        }

        public void Connect(IPort outputPort, IPort inputPort)
        {
            throw new System.NotImplementedException();
        }

        public void MarkDirty()
        {
            Debug.Log("[CEM] mark dirty");
        }

        #endregion
    }
}