using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Configs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Valkyrie.Entities
{
    public class EntitiesSerializer : IEntitiesSerializer
    {
        internal static readonly JsonSerializerSettings ComponentsSerializerSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private JsonSerializer ComponentsSerializer { get; } = new JsonSerializer()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        
        private readonly Dictionary<string, Func<JToken, IComponent>> _componentsFactory =
            new Dictionary<string, Func<JToken, IComponent>>();

        private readonly Dictionary<string, Func<IComponent, JToken>> _componentsWriters =
            new Dictionary<string, Func<IComponent, JToken>>();

        public void RegisterComponent(Type type)
        {
            var componentId = GetComponentId(type);

            JToken SerializeJsonComponent(IComponent c) => new JObject { { componentId, JToken.FromObject(c, ComponentsSerializer) } };
            IComponent DeserializeJsonComponent(JToken token) => (IComponent)token.ToObject(type);

            JToken SerializeEmptyComponent(IComponent c) => new JValue(componentId);
            IComponent CreateEmptyComponent(JToken token) => (IComponent)Activator.CreateInstance(type);

            JToken SerializeTypeValueProperty(IComponent c, PropertyInfo propertyInfo) => new JObject()
                { { componentId, JToken.FromObject(propertyInfo.GetValue(c), ComponentsSerializer) } };

            IComponent DeserializeTypeValueProperty(JToken token, PropertyInfo propertyInfo)
            {
                var r = (IComponent)Activator.CreateInstance(type);
                propertyInfo.SetValue(r, token.ToObject(propertyInfo.PropertyType));
                return r;
            }

            JToken SerializeTypeValueField(IComponent c, FieldInfo propertyInfo) => new JObject()
                { { componentId, JToken.FromObject(propertyInfo.GetValue(c), ComponentsSerializer) } };

            IComponent DeserializeTypeValueField(JToken token, FieldInfo propertyInfo)
            {
                var r = (IComponent)Activator.CreateInstance(type);
                propertyInfo.SetValue(r, token.ToObject(propertyInfo.FieldType));
                return r;
            }

            Func<IComponent, JToken> writeFunc;
            Func<JToken, IComponent> readFunc;
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public
                                                                      | BindingFlags.GetProperty
                                                                      | BindingFlags.SetProperty);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public
                                                              | BindingFlags.GetField
                                                              | BindingFlags.SetField);
            var totalCount = properties.Length + fields.Length;
            switch (totalCount)
            {
                case 0:
                    readFunc = CreateEmptyComponent;
                    writeFunc = SerializeEmptyComponent;
                    break;
                case 1:
                    if (properties.Length > 0)
                    {
                        readFunc = t => DeserializeTypeValueProperty(t, properties[0]);
                        writeFunc = c => SerializeTypeValueProperty(c, properties[0]);
                    }
                    else
                    {
                        readFunc = t => DeserializeTypeValueField(t, fields[0]);
                        writeFunc = c => SerializeTypeValueField(c, fields[0]);
                    }

                    break;
                default:
                    readFunc = DeserializeJsonComponent;
                    writeFunc = SerializeJsonComponent;
                    break;
            }

            _componentsFactory[componentId] = readFunc;
            _componentsWriters[componentId] = writeFunc;
        }

        JObject Serialize(Entity e)
        {
            var j = new JObject
            {
                { "Id", JToken.FromObject(e.Id) }
            };
            if (e._templates.Count > 0)
                j.Add("Templates", new JArray(e._templates.Select(x => x.Id)));
            if (e._components.Count > 0)
            {
                var arr = new JArray();
                foreach (var component in e._components)
                {
                    var cId = GetComponentId(component.GetType());
                    if (_componentsWriters.TryGetValue(cId, out var serializer))
                        arr.Add(serializer.Invoke(component));
                }

                j.Add("Components", arr);
            }

            if (e._slots.Count > 0)
                j.Add("Slots", JObject.FromObject(e._slots.ToDictionary(
                    x => x.Key,
                    x => x.Value.Id), ComponentsSerializer));

            return j;
        }

        public string Serialize(Entity e, Formatting formatting)
        {
            return Serialize(e).ToString(formatting);
        }

        public string Serialize(IEnumerable<Entity> es, Formatting formatting = Formatting.Indented)
        {
            return new JObject() { { "Entities", new JArray(es.Select(Serialize)) } }.ToString(formatting);
        }

        public Action Deserialize(EntitiesContext entitiesContext, string jsonText)
        {
            var calls = new List<Action>();
            var j = JObject.Parse(jsonText);
            if (j["Entities"] is JArray list)
                calls.AddRange(list.Select(x => ParseEntity(entitiesContext, x)));
            else
                calls.Add(ParseEntity(entitiesContext, j));
            return () => calls.ForEach(x => x.Invoke());
        }

        public static string GetComponentId(Type type) => type.Name.Replace("Component", string.Empty);

        private Action ParseEntity(EntitiesContext entitiesContext, JToken j)
        {
            var id = j["Id"]?.ToString();
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"[LOAD]: Not an entity {j}");
                return () => { };
            }

            var e = entitiesContext.GetEntity(id);
            if (e == null)
                entitiesContext.Add(e = new Entity(id));
            e._finishLoadAction = () => Fill(entitiesContext, e, j);
            return e.FinishLoading;
        }

        private void Fill(EntitiesContext entitiesContext, Entity entity, JToken j)
        {
            entity._templates.Clear();
            var templates = j["Templates"] ?? j["Parents"] ?? j["templates"] ?? j["parents"];
            if (templates != null)
                entity._templates.AddRange(templates.Values<string>().Select(x => entitiesContext.GetEntity(x, true)));

            entity._slots.Clear();
            var slots = j["Slots"] ?? j["slots"];
            if (slots != null)
            {
                var d = slots.ToObject<Dictionary<string, string>>();
                foreach (var pair in d) 
                    entity.AddSlot(pair.Key, entitiesContext.GetEntity(pair.Value, true));
            }
            
            foreach (var oEntity in entity._templates) oEntity.FinishLoading();
            foreach (var oEntity in entity._slots.Values) oEntity.FinishLoading();
            
            entity._components.Clear();
            foreach (var oEntity in entity._templates)
            foreach (var component in oEntity.CollectComponents(false))
                entity._components.Add(component.MakeCopy());

            var components = j["Components"] ?? j["components"];
            if (components is JArray list)
            {
                foreach (var jsonComponent in list)
                {
                    if (jsonComponent is JValue valueToken)
                    {
                        var componentId = valueToken.Value<string>();
                        if (_componentsFactory.TryGetValue(componentId, out var factory))
                            entity.AddComponent(factory.Invoke(valueToken));
                        else
                            Debug.LogWarning($"[LOAD]: unknown component {componentId} on entity{entity.Id}");
                    }
                    else
                    {
                        var jj = (JProperty)jsonComponent.Children().First();

                        var componentId = jj.Name;
                        if (_componentsFactory.TryGetValue(componentId, out var factory))
                            entity.AddComponent(factory.Invoke(jj.Value));
                        else
                            Debug.LogWarning($"[LOAD]: unknown component {componentId} on entity{entity.Id}");
                    }
                }
            }
            
            
            Debug.Log($"[LOAD]: loaded entity {entity}");
        }
    }
}