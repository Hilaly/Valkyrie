using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using Valkyrie.Meta.Configs;

namespace Valkyrie.Entities
{
    public static class EntitiesExtensions
    {
        public static void RegisterComponentsFromCurrentDomain(this IEntitiesSerializer serializer)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                serializer.RegisterComponents(assembly);
        }

        public static void RegisterComponents(this IEntitiesSerializer serializer, Assembly assembly)
        {
            serializer.RegisterComponents(assembly.GetTypes().Where(x =>
                !x.IsAbstract && x.IsClass &&
                typeof(IComponent).IsAssignableFrom(x)).ToArray());
        }

        public static void RegisterComponent<T>(this IEntitiesSerializer serializer) where T : IComponent =>
            serializer.RegisterComponent(typeof(T));

        public static void RegisterComponents(this IEntitiesSerializer serializer, params Type[] types)
        {
            foreach (var type in types)
                serializer.RegisterComponent(type);
        }

        public static Entity AddTemplate(this EntitiesContext ctx, Entity entity, params string[] templates)
        {
            foreach (var template in templates)
            {
                var et = ctx.GetEntity(template);
                if (et != null)
                    entity._templates.Add(et);
                else
                    Debug.LogWarning($"Couldn't find template {template}");
            }

            return entity;
        }

        public static IComponent MakeCopy(this IComponent component) =>
            (IComponent)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(component), component.GetType());

        public static Entity BuildFromTemplate(this EntitiesContext ctx, Entity template)
        {
            var r = new Entity(template.Id);
            r._templates.Add(template);
            foreach (var slot in template._slots)
                r._slots.Add(slot.Key, slot.Value);
            foreach (var component in template.CollectComponents())
                r.AddComponent(MakeCopy(component));


            ctx.Add(r);
            return r;
        }

        public static T GetOrCreate<T>(this Entity e) where T : IComponent, new()
        {
            var r = e.GetComponent<T>();
            if (r == null) e.AddComponent(r = new T());
            return r;
        }

        public static IReadOnlyList<Entity> GetOfType<TComponent>(this IConfigService configService)
            where TComponent : IComponent
        {
            return configService.Get<Entity>().Where(x => x.HasComponent<TComponent>()).ToList();
        }
    }
}