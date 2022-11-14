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
    }

    class NativeTypeArchetype : IArchetypeInfo
    {
        public readonly Type Type;
        
        private readonly List<IPropertyInfo> _properties;

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
    }
}