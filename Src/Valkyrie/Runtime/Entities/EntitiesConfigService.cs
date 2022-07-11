using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Configs;
using UnityEngine;

namespace Valkyrie.Entities
{
    public class EntitiesConfigService : EntitiesSerializer, IConfigLoader
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
            
            Debug.LogWarning(Serialize(Context.GetEntities(true)));
            Debug.Log($"[LOAD]: {Context.Count} entities loaded");

            return Task.FromResult((IEnumerable<IConfigData>)Context.GetEntities(true));
        }
    }
}