using System;
using System.Collections.Generic;
using System.Reflection;
using Utils;

namespace Valkyrie.Utils
{
    public class TypeCache<TSearch, TStorage> where TStorage : Attribute
    {
        private bool _initialized;
        private readonly BuildAction _buildAction;
        private readonly Dictionary<Type, TStorage> _cache;

        public delegate bool BuildAction(Type type, ref TStorage storage, out Type key);

        public TypeCache(BuildAction buildAction)
        {
            _buildAction = buildAction;
            _initialized = false;
            _cache = new Dictionary<Type, TStorage>();
        }

        public IEnumerable<TStorage> All
        {
            get
            {
                ShouldBuildCache();
                return _cache.Values;
            }
        }
        
        public TStorage this[Type key]
        {
            get
            {
                ShouldBuildCache();
                return _cache.TryGetValue(key, out var result) ? result : default;
            }
        }

        public TStorage Get<T>() => this[typeof(T)];

        public bool TryGet(Type type, out TStorage output)
        {
            ShouldBuildCache();
            return _cache.TryGetValue(type, out output);
        }

        private void ShouldBuildCache()
        {
            if (_initialized)
                return;

            BuildCache();
        }

        private void BuildCache()
        {
            foreach (var type in typeof(TSearch).GetAllSubTypes(x => true))
            {
                foreach (var attribute in type.GetCustomAttributes<TStorage>())
                {
                    var storage = attribute;
                    if (_buildAction(type, ref storage, out var key))
                    {
                        _cache.Add(key, storage);
                    }
                }
            }

            _initialized = true;
        }
    }
}