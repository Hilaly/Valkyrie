using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Valkyrie
{
    public partial class WorldModelInfo
    {
        internal List<Type> RegisteredSystems = new();
        
        public WorldModelInfo ImportSystem<T>() where T : ISimSystem => ImportSystem(typeof(T));

        public WorldModelInfo ImportSystem(Type type)
        {
            if (!typeof(ISimSystem).IsAssignableFrom(type))
                throw new Exception($"{type.FullName} is not convertible to ISimSystem");
            if (!RegisteredSystems.Contains(type)) RegisteredSystems.Add(type);

            return this;
        }
        
        public EntityType ImportEntity<T>() where T : IEntity => ImportEntity(typeof(T));

        public EntityType ImportEntity(Type typeInstance)
        {
            if (!typeInstance.IsInterface)
                throw new Exception("Allow import only interfaces");
            if (!typeof(IEntity).IsAssignableFrom(typeInstance))
                throw new Exception("Interface must be convertible to IEntity");

            Debug.Log($"[CEM] registering {typeInstance.FullName}");

            var e = CreateEntity(typeInstance.FullName);
            e.AddAttribute("native");
            foreach (var inherited in typeInstance.GetInterfaces())
            {
                if(inherited == typeof(IEntity))
                    continue;
                if (!typeof(IEntity).IsAssignableFrom(inherited))
                    throw new Exception("Allow inherit only from entities");
                var ex = Get<EntityType>(inherited.FullName);
                if (ex == null)
                    throw new Exception($"{inherited.FullName} is not registered entity");

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

            return e;
        }
    }
}