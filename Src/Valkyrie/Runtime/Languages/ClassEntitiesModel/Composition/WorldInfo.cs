using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Valkyrie.Composition
{
    public interface IFeature
    {
        void Register(IWorldInfo worldInfo);
    }

    public interface IWorldInfo
    {
        string Namespace { get; }

        IReadOnlyList<IComponentInfo> GetComponents();
        IReadOnlyList<IArchetypeInfo> GetArchetypes();
        IReadOnlyList<ISystemInfo> GetSystems();

        IWorldInfo RegisterArchetype(Type type);
        IWorldInfo RegisterSystem(Type type);
    }

    public class WorldInfo : IWorldInfo
    {
        private readonly Dictionary<string, IArchetypeInfo> _archetypes = new();
        private readonly Dictionary<string, ISystemInfo> _systens = new();

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

        public IReadOnlyList<IArchetypeInfo> GetArchetypes() => _archetypes.Values.ToList();
        public IReadOnlyList<ISystemInfo> GetSystems() => _systens.Values.ToList();

        public IWorldInfo RegisterArchetype(Type type)
        {
            var typeName = type.FullName;
            if (!_archetypes.TryGetValue(typeName, out _))
                _archetypes.Add(typeName, new NativeTypeArchetype(type));
            return this;
        }

        public IWorldInfo RegisterSystem(Type type)
        {
            var typeName = type.FullName;
            if (!_systens.TryGetValue(typeName, out _))
                _systens.Add(typeName, new NativeTypeSystem(type));
            return this;
        }
    }
}