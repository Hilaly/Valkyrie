using System.Collections.Generic;
using System.Linq;

namespace Valkyrie
{
    public class Feature : MainFeatureData
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
    }
}