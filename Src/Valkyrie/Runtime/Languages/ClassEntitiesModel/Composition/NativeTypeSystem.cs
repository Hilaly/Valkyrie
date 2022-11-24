using System;
using System.Collections.Generic;
using System.Linq;
using Valkyrie.Ecs;

namespace Valkyrie.Composition
{
    public interface ISystemInfo
    {
        public string Name { get; }
        bool NeedInit { get; }

        public IReadOnlyList<KeyValuePair<IArchetypeInfo, ArchetypeUsageInSystem>> GetSimulation();
    }

    public enum ArchetypeUsageInSystem
    {
        Single,
        Multiple,
        Event
    }

    class NativeTypeSystem : ISystemInfo
    {
        private readonly List<KeyValuePair<IArchetypeInfo, ArchetypeUsageInSystem>> _archetypeUsages = new();

        public Type Type;

        public string Name => Type.FullName.ToFullName();

        public bool NeedInit => typeof(IEcsInitSystem).IsAssignableFrom(Type);

        public IReadOnlyList<KeyValuePair<IArchetypeInfo, ArchetypeUsageInSystem>> GetSimulation()
            => _archetypeUsages;

        public NativeTypeSystem(Type type, IWorldInfo worldInfo)
        {
            if (!typeof(ISharedSystem).IsAssignableFrom(type))
                throw new Exception($"{type.FullName} not convertible to ISharedSystem");

            Type = type;

            _archetypeUsages = worldInfo.CollectArchetypeUsagesFromSystem(type).ToList();
        }
    }
}