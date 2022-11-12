using System;
using System.Reflection;
using UnityEngine;

namespace Valkyrie
{
    public partial class WorldModelInfo
    {
        public EntityType Import<T>() where T : IEntity => ImportEntity(typeof(T));

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
                Debug.Log(
                    $"[CEM] {e.Name} has property {propertyInfo.Name} of type {propertyInfo.PropertyType.FullName}");
                e.AddProperty(propertyInfo.PropertyType, propertyInfo.Name,
                    propertyInfo.GetCustomAttribute<RequiredPropertyAttribute>() != null);
            }

            return e;
        }
    }
}