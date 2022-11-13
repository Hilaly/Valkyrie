using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Valkyrie.Composition
{
    public interface IComponentInfo
    {
        string Name { get; }
        
        string GetTypeName();
    }

    class NativeTypeComponent : IComponentInfo
    {
        public Type Type;
        public string Name { get; set; }

        public string GetTypeName() => Type.FullName;
    }
    
    public interface IFeature
    {
        void Register(IWorldInfo worldInfo);
    }
    
    public class WorldInfo : IWorldInfo
    {
        public string Namespace { get; set; } = "Generated";
        
        public IReadOnlyList<IComponentInfo> GetComponents()
        {
            var d = new Dictionary<string, IComponentInfo>();
            foreach (var archetype in GetArchetypes())
            foreach (var property in archetype.Properties)
            {
                if (d.TryGetValue(property.Name, out var exist))
                    if (exist.GetTypeName() != property.GetTypeName())
                        throw new Exception(
                            $"founded different components {property.Name} -> {exist.GetTypeName()} != {property.GetTypeName()}");
                d.Add(property.Name, property);
            }

            return d.Values.ToList();
        }

        public IReadOnlyList<IArchetypeInfo> GetArchetypes()
        {
            return new IArchetypeInfo[]
            {
                new NativeTypeArchetype(typeof(Test))
            };
        }
    }

    public interface Test : IEntity
    {
        public bool GetMarker { get; }
        public bool SetMarker { set; }
        public bool Marker { get; set; }
        
        public Vector3 Position { get; set; }
        public Vector3 GetPosition { get; }
        public Vector3 SetPosition { set; }
        
        public ITimer Timer { get; }
    }

    public interface IWorldInfo
    {
        string Namespace { get; }

        IReadOnlyList<IComponentInfo> GetComponents();
        IReadOnlyList<IArchetypeInfo> GetArchetypes();
    }
}