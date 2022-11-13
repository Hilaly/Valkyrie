using System;
using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Composition
{
    public interface IArchetypeInfo
    {
        public string Name { get; }
        public IReadOnlyList<IPropertyInfo> Properties { get; }
    }

    class NativeTypeArchetype : IArchetypeInfo
    {
        public readonly Type Type;
        
        private readonly List<IPropertyInfo> _properties;

        public NativeTypeArchetype(Type type)
        {
            if (!type.IsInterface)
                throw new Exception($"{type.FullName} is not interface");
            if (!typeof(IEntity).IsAssignableFrom(type))
                throw new Exception($"{type.FullName} can not be converted to IEntity");
            
            Type = type;

            _properties = type.CollectArchetypeProperties().ToList();
        }

        public string Name => Type.FullName.ToFullName();

        public IReadOnlyList<IPropertyInfo> Properties => _properties;
    }
}