using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Valkyrie.Composition
{
    class NativeTypeArchetype : IArchetypeInfo
    {
        public readonly Type Type;
        
        private readonly List<IPropertyInfo> _properties;

        public NativeTypeArchetype(Type type)
        {
            Type = type;

            _properties = type.CollectProperties().ToList();
        }

        public string Name => Type.FullName;

        public IReadOnlyList<IPropertyInfo> Properties => _properties;
    }

    class NativePropertyInfo : IPropertyInfo
    {
        public readonly PropertyInfo PropertyInfo;

        public NativePropertyInfo(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
        }

        public string Name => PropertyInfo.Name;

        public string GetTypeName() => PropertyInfo.PropertyType.FullName;

        public bool IsWriteEnabled => PropertyInfo.CanWrite;
        public bool IsReadEnabled => PropertyInfo.CanRead;
    }
    
    public interface IPropertyInfo : IComponentInfo
    {
        bool IsWriteEnabled { get; }
        bool IsReadEnabled { get; }
    }

    public interface IArchetypeInfo
    {
        public string Name { get; }
        public IReadOnlyList<IPropertyInfo> Properties { get; }
    }

}