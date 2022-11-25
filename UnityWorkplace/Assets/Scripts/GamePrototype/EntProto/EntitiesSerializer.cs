using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Utils;
using Valkyrie.MVVM;

namespace NaiveEntity.GamePrototype.EntProto
{
    public class EntitiesSerializer
    {
        private List<Type> _allTypes;
        private readonly Dictionary<string, Type> _shortCache = new Dictionary<string, Type>();

        Type FindType(string name)
        {
            if (!_shortCache.TryGetValue(name, out var r))
            {
                _allTypes ??= typeof(IComponent).GetAllSubTypes(x => !x.IsAbstract && x.IsClass);

                _shortCache.Add(name, r = _allTypes.Find(x => x.Name == name));
            }

            return r;
        }

        public void Deserialize(EntityContext context, string json)
        {
            var jo = JObject.Parse(json);
            var deserializeReferences = new List<Action>();

            var allE = jo["Entities"] ?? jo["entities"];
            foreach (var token in allE)
            {
                IEntity e = null;
                if (token["Id"] != null)
                {
                    var id = token["Id"].Value<string>();
                    e = context.Get(id) ?? context.Create(id);
                }

                if (e == null)
                    e = context.Create(Guid.NewGuid().ToString());

                DeserializeComponents((Entity)e, token);

                var containers = token["Containers"] ?? token["containers"];
                if (containers != null)
                    deserializeReferences.Add(() => DeserializeContainers((Entity)e, containers, context));
            }

            foreach (var action in deserializeReferences)
                action();
        }

        private void DeserializeContainers(Entity entity, JToken token, EntityContext context)
        {
            foreach (var child in token.Children<JProperty>())
            {
                var containerName = child.Name;
                var entityIds = child.Value.Values<string>();
                foreach (var entityId in entityIds)
                {
                    var innerEntity = context.Get(entityId);
                    if (innerEntity != null)
                        entity.AddToContainer(containerName, innerEntity);
                    else
                        Debug.LogWarning($"Couldn't find entity {entityId}");
                }
            }
        }

        private void DeserializeComponents(Entity entity, JToken token)
        {
            var componentsJson = token["Components"] ?? token["components"];
            if (componentsJson == null)
                return;
            foreach (var jsonComponent in componentsJson)
            {
                //Simple flag component, just name in JSON
                if (jsonComponent is JValue valueToken)
                {
                    var componentId = valueToken.Value<string>();
                    var t = FindType(componentId) ?? FindType($"{componentId}Component");
                    if (t != null)
                        entity.AddComponent(Activator.CreateInstance(t));
                    else
                        Debug.LogWarning($"Unknown component {componentId}");
                }
                else
                {
                    var jj = (JProperty)jsonComponent.Children().First();
                    var componentId = jj.Name;
                    var value = jj.Value;
                    var t = FindType(componentId) ?? FindType($"{componentId}Component");
                    if (t != null)
                    {
                        var c = Activator.CreateInstance(t);
                        var f = t.GetFields(BindingFlags.Public | BindingFlags.Instance)[0];
                        f.SetValue(c, value.ToObject(f.FieldType));
                        entity.AddComponent(c);
                    }
                    else
                        Debug.LogWarning($"Unknown component {componentId}");
                }
            }
        }
    }
}