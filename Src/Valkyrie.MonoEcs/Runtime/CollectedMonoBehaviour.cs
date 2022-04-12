using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.Ecs
{
    public class CollectedMonoBehaviour<T> : MonoBehaviour
        where T : CollectedMonoBehaviour<T>
    {
        private static readonly CachedList<T> _allEntities = new();
        private static readonly CachedList<T> _allActive = new();

        protected static List<T> All => _allEntities.Get();
        protected static List<T> Active => _allActive.Get();

        public static List<T> GetAll() => Active;

        protected void OnEnable() => _allActive.Add((T)this);
        protected void OnDisable() => _allActive.Remove((T)this);
        protected virtual void OnDestroy() => _allEntities.Remove((T)this);
        protected virtual void Awake() => _allEntities.Add((T)this);
    }
}