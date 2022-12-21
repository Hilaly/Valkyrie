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
            foreach (var componentInfo in archetype.Components)
            {
                if (d.TryGetValue(componentInfo.Name, out var exist))
                    if (exist.GetTypeName() != componentInfo.GetTypeName())
                        throw new Exception(
                            $"founded different components {componentInfo.Name} -> {exist.GetTypeName()} != {componentInfo.GetTypeName()}");
                d.Add(componentInfo.Name, componentInfo);
            }

            return d.Values.ToList();
        }

        public IReadOnlyList<IArchetypeInfo> GetArchetypes() => _archetypes.Values.ToList();
        public IReadOnlyList<ISystemInfo> GetSystems() => _systens.Values.ToList();

        public IWorldInfo RegisterArchetype(Type type)
        {
            var typeName = type.FullName;
            if (!_archetypes.TryGetValue(typeName, out _))
            {
                _archetypes.Add(typeName, CreateArchetype(type));
            }
            return this;
        }

        private static IArchetypeInfo CreateArchetype(Type type)
        {
            if (!type.IsInterface)
                throw new Exception($"{type.FullName} is not interface");
            if (!typeof(IEntity).IsAssignableFrom(type))
                throw new Exception($"{type.FullName} can not be converted to IEntity");
            if (typeof(IEventEntity).IsAssignableFrom(type))
                return new NativeTypeEventArchetype(type);
            return new NativeTypeArchetype(type);
        }

        public IWorldInfo RegisterSystem(Type type)
        {
            var typeName = type.FullName;
            if (!_systens.TryGetValue(typeName, out _))
            {
                GetAllExportedTypesFromSystem(type).ForEach(x => RegisterArchetype(x));
                _systens.Add(typeName, new NativeTypeSystem(type, this));
            }
            return this;
        }

        List<Type> GetAllExportedTypesFromSystem(Type type) =>
            this.CollectArchetypeUsagesFromSystem(type)
                .Where(x => x.Key is NativeTypeArchetype)
                .Select(x => ((NativeTypeArchetype)x.Key).Type)
                .ToList();
    }
}