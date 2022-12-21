using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Valkyrie.Composition
{
    public interface IArchetypeFilter
    {
        IReadOnlyList<string> Required { get; }
        IReadOnlyList<string> Excluded { get; }
    }
    
    public interface IArchetypeInfo : IArchetypeFilter
    {
        public string Name { get; }
        public IReadOnlyList<IPropertyInfo> Properties { get; }
        public IReadOnlyList<IComponentInfo> Components { get; }
    }
    
    class NativeTypeEventArchetype : IArchetypeInfo, IComponentInfo
    {
        private readonly List<NativePropertyInfo> _properties;
        
        public Type Type { get; }
        public string Name => Type.FullName.ToFullName();

        string IComponentInfo.Name => Name.Clean();

        public IReadOnlyList<string> Required => new[] { ((IComponentInfo)this).Name };
        public IReadOnlyList<string> Excluded { get; } = new List<string>();

        public IReadOnlyList<IPropertyInfo> Properties => _properties;

        public IReadOnlyList<IComponentInfo> Components => new IComponentInfo[] { this };

        public NativeTypeEventArchetype(Type type)
        {
            if (!type.IsInterface)
                throw new Exception($"{type.FullName} is not interface");
            if (!typeof(IEventEntity).IsAssignableFrom(type))
                throw new Exception($"{type.FullName} can not be converted to IEventEntity");
            
            _properties = type.CollectArchetypeProperties().ToList();
            
            Type = type;
        }
        
        public string GetTypeName()
        {
            return Type.FullName;
        }
    }

    class NativeTypeArchetype : IArchetypeInfo
    {
        public readonly Type Type;
        
        private readonly List<NativePropertyInfo> _properties;

        public IReadOnlyList<string> Required { get; }
        public IReadOnlyList<string> Excluded { get; }

        public NativeTypeArchetype(Type type)
        {
            if (!type.IsInterface)
                throw new Exception($"{type.FullName} is not interface");
            if (!typeof(IEntity).IsAssignableFrom(type))
                throw new Exception($"{type.FullName} can not be converted to IEntity");
            
            Type = type;

            _properties = type.CollectArchetypeProperties().ToList();
            Required = type
                .GetCustomAttributes<RequiredPropertyAttribute>()
                .SelectMany(x => x.Properties)
                .ToHashSet()
                .OrderBy(x => x)
                .ToList();
            Excluded = type
                .GetCustomAttributes<ExcludePropertyAttribute>()
                .SelectMany(x => x.Properties)
                .ToHashSet()
                .OrderBy(x => x)
                .ToList();
        }

        public string Name => Type.FullName.ToFullName();

        public IReadOnlyList<IPropertyInfo> Properties => _properties;
        public IReadOnlyList<IComponentInfo> Components => _properties;
    }
}