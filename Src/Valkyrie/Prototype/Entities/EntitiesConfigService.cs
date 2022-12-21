using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Valkyrie.Meta.Configs;

namespace Valkyrie.Entities
{
    public class EntitiesConfigService : EntitiesSerializer
    {
        public EntitiesContext Context { get; } = new EntitiesContext(null);

        public Task<IEnumerable<IConfigData>> Load()
        {
            var actions = new List<Action>();
            foreach (var resource in Resources.LoadAll<TextAsset>("Json"))
            {
                Debug.Log($"[LOAD]: Loading entities from {resource.name}");
                actions.Add(Deserialize(Context, resource.text));
            }

            foreach (var action in actions)
                action();

            Debug.LogWarning(Serialize(Context.GetEntities()));
            Debug.Log($"[LOAD]: {Context.GetEntities().Count} entities loaded");

            return Task.FromResult((IEnumerable<IConfigData>)Context.GetEntities());
        }
    }
}