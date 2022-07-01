using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.Ecs
{
    public class CollectedMonoBehaviour<T> : MonoBehaviour
        where T : CollectedMonoBehaviour<T>
    {
        private static readonly CachedList<T> AllEntities = new();
        private static readonly CachedList<T> AllActive = new();

        protected static List<T> All => AllEntities.Get();
        protected static List<T> Active => AllActive.Get();

        public static List<T> GetAll() => Active;

        protected void OnEnable() => AllActive.Add((T)this);
        protected void OnDisable() => AllActive.Remove((T)this);
        protected virtual void OnDestroy() => AllEntities.Remove((T)this);
        protected virtual void Awake() => AllEntities.Add((T)this);
    }
}