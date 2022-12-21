using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Utils;

namespace Valkyrie.Meta
{
    public interface IDataStorage<in T>
    {
        TC Get<TC>(string id) where TC : T;
        IReadOnlyList<TC> Get<TC>() where TC : T;
    }

    class DataStorage<T> : IDataStorage<T>
    {
        private Dictionary<string, T> _dictionary = new();
        private readonly Dictionary<Type, object> _allCache = new();

        protected internal Dictionary<string, T> Dictionary => _dictionary;

        protected void Load(string text)
        {
            _allCache.Clear();
            _dictionary =
                JsonConvert.DeserializeObject<Dictionary<string, T>>(text, DataExtensions.StandardJsonSettings);

            System.Diagnostics.Debug.Assert(_dictionary != null, nameof(_dictionary) + " != null");


            foreach (var pair in _dictionary)
                AddToCache(pair.Value);
        }

        public TC Get<TC>(string id) where TC : T
        {
            if (_dictionary.TryGetValue(id, out var value) && value is TC t)
                return t;
            Debug.LogWarning($"'Couldn't find {typeof(TC).Name} id={id}");
            return default;
        }

        public IReadOnlyList<TC> Get<TC>() where TC : T =>
            _allCache.TryGetValue(typeof(TC), out var r) ? (IReadOnlyList<TC>)r : ArraySegment<TC>.Empty;

        protected internal void Add<TC>(TC data, string id) where TC : T
        {
            _dictionary.Add(id, data);

            AddToCache(data);
        }

        private void AddToCache(T data)
        {
            void AddToTypedCache(Type type)
            {
                if (!_allCache.TryGetValue(type, out var temp))
                    _allCache.Add(type, temp = Activator.CreateInstance(typeof(List<>).MakeGenericType(type)));
                ((IList)temp).Add(data);
            }

            var t = data.GetType();
            while (t is { IsClass: true })
            {
                AddToTypedCache(t);
                t = t.BaseType;
            }
        }

        public void Clear()
        {
            _allCache.Clear();
            _dictionary.Clear();
        }
    }
}