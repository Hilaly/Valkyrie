using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utils.Pool
{
    public class ObjectsPool : IObjectsPool, IDisposable
    {
        private readonly Dictionary<string, List<GameObject>> _instances = new();
        private readonly Dictionary<string, GameObject> _prefabs = new();

        public GameObject Root { get; }

        public ObjectsPool()
        {
            Root = new GameObject("ObjectsPool");
        }

        public IPooledInstance<T> Instantiate<T>(string prefabName) where T : Component
        {
            var instance = CreateInstance<T>(prefabName);
            return new Wrapper<T>(() => Release(instance, prefabName), instance);
        }

        public IPooledInstance<T> Instantiate<T>(string prefabName, Vector3 position, Quaternion rotation) where T : Component
        {
            var instance = Instantiate<T>(prefabName);
            instance.Instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        private void Release<T>(T instance, string prefabName) where T : Component
        {
            var gameObject = instance.gameObject;
            gameObject.SetActive(false);
            _instances[prefabName].Add(gameObject);
        }

        private T CreateInstance<T>(string prefabName) where T : Component
        {
            if (!_instances.TryGetValue(prefabName, out var readyInstances))
                _instances[prefabName] = readyInstances = new List<GameObject>();

            if (readyInstances.Count > 0)
            {
                var result = readyInstances[^1];
                readyInstances.RemoveAt(readyInstances.Count - 1);
                result.SetActive(true);
                return result.GetComponent<T>();
            }

            if (!_prefabs.TryGetValue(prefabName, out var prefab))
                _prefabs[prefabName] = prefab = Resources.Load<GameObject>(prefabName);

            return Object.Instantiate(prefab, Root.transform).GetComponent<T>();
        }

        public void Dispose() => Object.Destroy(Root);
    }
}