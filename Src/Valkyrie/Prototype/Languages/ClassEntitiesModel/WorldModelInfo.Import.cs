using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Valkyrie
{
    public partial class WorldModelInfo
    {
        internal readonly List<KeyValuePair<Type, int>> RegisteredSystems = new();

        public WorldModelInfo ImportSystem<T>(int order = SimulationOrder.Default) where T : ISimSystem =>
            ImportSystem(typeof(T), order);

        public WorldModelInfo ImportSystem(Type type, int order = SimulationOrder.Default)
        {
            if (!typeof(ISimSystem).IsAssignableFrom(type))
                throw new Exception($"{type.FullName} is not convertible to ISimSystem");
            if (RegisteredSystems.TrueForAll(x => x.Key != type))
                RegisteredSystems.Add(new KeyValuePair<Type, int>(type, order));

            return this;
        }

        public EntityType ImportEntity<T>() where T : IEntity => ImportEntity(typeof(T));

        public EntityType ImportEntity(Type typeInstance)
        {
            if (!typeInstance.IsInterface)
                throw new Exception("Allow import only interfaces");
            if (!typeof(IEntity).IsAssignableFrom(typeInstance))
                throw new Exception("Interface must be convertible to IEntity");

            var e = Get<EntityType>(typeInstance.FullName);
            if (e != null)
                return e;

            Debug.Log($"[CEM] registering {typeInstance.FullName}");

            e = CreateEntity(typeInstance.FullName);
            e.AddAttribute("native");
            foreach (var inherits in typeInstance.GetInterfaces())
            {
                if (inherits == typeof(IEntity))
                    continue;
                if (!typeof(IEntity).IsAssignableFrom(inherits))
                    throw new Exception("Allow inherit only from entities");
                var ex = Get<EntityType>(inherits.FullName) ?? ImportEntity(inherits);
                Debug.Log($"[CEM] {e.Name} inherited from {ex.Name}");
                e.Inherit(ex);
            }

            foreach (var propertyInfo in typeInstance.GetProperties(BindingFlags.Instance | BindingFlags.Public
                | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.DeclaredOnly))
            {
                var type = propertyInfo.PropertyType;
                var propName = propertyInfo.Name;
                var required = propertyInfo.GetCustomAttribute<RequiredPropertyAttribute>() != null;
                Debug.Log(
                    $"[CEM] {e.Name} has {(required ? "required" : string.Empty)} property {propName} of type {type.FullName}");
                e.AddProperty(type, propName, required);
            }

            foreach (var propertyInfo in typeInstance.GetProperties(BindingFlags.Instance | BindingFlags.Public
                | BindingFlags.GetProperty | BindingFlags.DeclaredOnly))
            {
                if (propertyInfo.PropertyType != typeof(ITimer))
                    continue;

                Debug.Log($"[CEM] {e.Name} has timer {propertyInfo.Name}");
                e.AddTimer(propertyInfo.Name);
            }

            return e;
        }
    }
}